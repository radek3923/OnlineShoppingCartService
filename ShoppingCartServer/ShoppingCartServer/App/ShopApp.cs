using System.IO.Pipes;
using System.Text.RegularExpressions;
using ShoppingCartServer.Enums;
using ShoppingCartServer.FileOperations;
using ShoppingCartServer.Models;
using ShoppingCartServer.Utils.Generators;
using ShoppingCartServer.Utils.Validators;
using ShoppingCartUser.Communication;

class ShopApp
{
    private const string AppName = "Sklep internetowy";
    
    private static List<Customer> _customers;
    private static List<Product> _products;
    private static List<Cart> _historyShoppingCarts;
    
    private static readonly Admin admin = new("admin", "admin", "admin.email.pl", "123456789", AccessLevel.Full);
    private static Customer loggedCustomer;

    public static async Task Main()
    {
        FileManager fileManager = new FileManager();
        DataGenerator dataGenerator = new DataGenerator();
        CancellationTokenSource cts = new CancellationTokenSource();
        
        Task timeoutTask = Task.Delay(60 * 1000); // 60 seconds
        
        var (importCustomersTask, importProductTask, importShoppingCartsHistoryTask) =
            (fileManager.importCustomers(), fileManager.importProducts(), fileManager.importShoppingCartsHistory());
        
        //
        // if (Task.WaitAny(new[] { importShoppingCartsHistoryTask, timeoutTask }, cts.Token) == 1)
        // {
        //     cts.Cancel();
        //     Console.WriteLine("Reading data from files took too long, cancellation requested.");
        // }
        
        
        _customers = await importCustomersTask;
        _products = await importProductTask;
        _historyShoppingCarts = await importShoppingCartsHistoryTask;
        
        // List<CartItem> cartItems = new List<CartItem>();
        // CartItem cartItem1 = new CartItem(new Guid(), new Guid(), 0);
        // CartItem cartItem2 = new CartItem(new Guid(), new Guid(), 0);
        // cartItems.Add(cartItem1);
        // cartItems.Add(cartItem2);
        
        // fileManager.saveObjectToDatabase(new Customer("test1", "test2", "test3", "test4", new Guid(), "test6", "test7"));
        // fileManager.saveObjectToDatabase(new Product(new Guid(), new DateTimeOffset(), new DateTimeOffset(), "2", "2", new decimal()));
        //fileManager.saveCartToDatabase(new Cart(new Guid(), new DateTimeOffset(), new DateTimeOffset(), new Guid(), cartItems ));
         
         // showAllList(_products);
         // showAllList(_customers);
         //showAllList(_historyShoppingCarts);
         
         // Console.WriteLine(dataGenerator.getNewGuID());
         // Console.WriteLine(dataGenerator.GetActualDateTimeOffset());

        var pipeServer = new NamedPipeServerStream("PipeName", PipeDirection.InOut, 2);
        var reader = new StreamReader(pipeServer);
        var writer = new StreamWriter(pipeServer);
        ClientCommunication clientCommunication = new ClientCommunication(State.Disconnected,reader, writer);
        Console.WriteLine("Oczekiwanie na połączenie z klientem");
        pipeServer.WaitForConnection();
        Console.Clear();
        Console.WriteLine("Połączono z klientem");
        clientCommunication.State = State.Connected;

        while (State.Connected.Equals(clientCommunication.State))
        {
            string[] data = clientCommunication.ReadData();
            Operation operation = Enum.Parse<Operation>(data[0]);

            switch (operation)
            {
                case Operation.Disconnect:
                    Console.WriteLine("Trwa wyłączanie serwera ");
                    clientCommunication.State = State.Disconnected;
                    break;
                case Operation.Login:
                    Console.WriteLine("Wybrano logowanie");
                    var login = data[1];
                    var password = data[2];

                    Console.WriteLine("Login: " + login);
                    Console.WriteLine("Haslo: " + password);

                     loggedCustomer = findCustomerInDatabase(login, password, _customers);

                    if (loggedCustomer is not null)
                    {
                        //it means user is Customer
                        clientCommunication.SendData(Operation.Login, "TRUE", "FALSE");
                        loggedCustomer= _customers.Where(p => p.Login == login && p.Password == password).First();
                        Console.WriteLine(loggedCustomer.ToString());
                        
                        // var productsAsString = await Task.Run((() => File.ReadAllText("products.csv")));
                        // productsAsString = Regex.Replace(productsAsString, "\n", "#");
                        // //productsAsString = Regex.Replace(productsAsString, "\r\n", "#");
                        string s1 = ";";
                        string s2 = "#";
                        string productsAsString ="";
                        foreach (var product in _products)
                        {
                            productsAsString += product.Id.ToString() + s1 + product.CreatedAt.ToString() + s1 + product.UpdatedAt.ToString() + s1 +
                                                product.Name + s1 + product.NamePlural + s1 + product.UnitPrice.ToString() + s2;
                            // Console.WriteLine(productsAsString);
                            // productsAsString += fileManager.objectToCsv(product) + s2;
                        }
                        productsAsString = productsAsString.Remove(productsAsString.Length-1);
                        Console.WriteLine(productsAsString);
                        clientCommunication.SendData(Operation.SendingProducts, productsAsString);
                        Console.WriteLine("Przeslano");
                    }
                    else if (admin.Login.Equals(login) && admin.Password.Equals(password))
                    {
                        //it means user is Admin
                        clientCommunication.SendData(Operation.Login, "TRUE", "TRUE");
                        var productsAsString = await Task.Run((() => File.ReadAllText("products.csv")));
                        productsAsString = Regex.Replace(productsAsString, "\n", "#");
                        //productsAsString = Regex.Replace(productsAsString, "\r\n", "#");
                        clientCommunication.SendData(Operation.SendingProducts, productsAsString);
                    }
                    else
                    {
                        // it means user not found in database and user is not admin
                        clientCommunication.SendData(Operation.Login, "FALSE", "FALSE");
                    }
                    break;
                case Operation.Register:
                    
                    Console.WriteLine("Uzytkownik wybral rejestracje");
                    
                    string loginNew = data[1];
                    string passwordNew = data[2];
                    string addressEmailNew = data[3];
                    string phoneNumberNew = data[4];
                    string firstNameNew = data[5];
                    string lastNameNew = data[6];

                    DataValidator dataValidator = new DataValidator();

                    if (dataValidator.isDataValid(loginNew, addressEmailNew, phoneNumberNew, _customers))
                    {
                        Customer newCustomer = new Customer(loginNew, passwordNew, addressEmailNew, phoneNumberNew, dataGenerator.getNewGuID(), firstNameNew, lastNameNew );
                        _customers.Add(newCustomer);
                        fileManager.saveObjectToDatabase(newCustomer);
                    }
                    break;
                case Operation.Buy:
                    // data 
                    List<CartItem> cartItems = new List<CartItem>();
                    Cart cart = new Cart(dataGenerator.getNewGuID(), new DateTimeOffset(), new DateTimeOffset(),loggedCustomer.Id, cartItems );
                    for (int i = 1; i < data.Length; i++)
                    {
                        var dataSpilted = data[i].Split("&");
                        CartItem cartItem = new CartItem(dataGenerator.getNewGuID(), Guid.Parse(dataSpilted[0]),
                            int.Parse(dataSpilted[2]));
                        cartItems.Add(cartItem);    
                    }
                    cart.Products = cartItems;
                    // zapisywanie koszyka do bazy danych
                    break;
                case Operation.AddProducts:
                    for (int i = 1; i < data.Length; i++)
                    {
                        var dataSpilted = data[i].Split("&");
                        fileManager.saveObjectToDatabase(new Product(dataGenerator.getNewGuID(), dataGenerator.GetActualDateTimeOffset(),
                            dataGenerator.GetActualDateTimeOffset(), dataSpilted[0], dataSpilted[1], int.Parse(dataSpilted[2])));
                    }

                    break;
                case Operation.RemoveProducts:
                    for (int i = 0; i < data.Length; i++)
                    {
                        Console.WriteLine(data[i]);
                        //var dataSpilted = data[i].Split("&");
                    }
                    break;
            }
        }

        pipeServer.Close();
    }

    public static Customer findCustomerInDatabase(String login, String password, List<Customer> _customers)
    {
        var customer = _customers.Where((customer) => (customer.Login == login && customer.Password == password))
            .FirstOrDefault() ?? null;
        return customer;
    }

    public static void showAllList<T>(List<T> list)
    {
        foreach (var item in list)
        {
            Console.WriteLine(item);
        }
    }
}