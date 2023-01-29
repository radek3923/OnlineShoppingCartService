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
        
        var (importCustomersTask, importProductTask, importShoppingCartsHistoryTask) =
            (fileManager.importCustomers(), fileManager.importProducts(), fileManager.importShoppingCartsHistory());
        
        _customers = await importCustomersTask;
        _products = await importProductTask;
        _historyShoppingCarts = await importShoppingCartsHistoryTask;
        
         // fileManager.saveObjectToDatabase(new Customer("test1", "test2", "test3", "test4", new Guid(), "test6", "test7"));
         // fileManager.saveObjectToDatabase(new Product(new Guid(), new DateTimeOffset(), new DateTimeOffset(), "2", "2", new decimal()));
         
         // showAllList(_products);
         // showAllList(_customers);
         showAllList(_historyShoppingCarts);
         
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
            Console.WriteLine("dlugosc tab " + data.Length);
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

                        var productsAsString = await Task.Run((() => File.ReadAllText("products.csv")));
                        //productsAsString = Regex.Replace(productsAsString, "\n", "#");
                        productsAsString = Regex.Replace(productsAsString, "\r\n", "#");
                        clientCommunication.SendData(Operation.SendingProducts, productsAsString);
                        Console.WriteLine("Przeslano");
                    }
                    else if (admin.Login.Equals(login) && admin.Password.Equals(password))
                    {
                        //it means user is Admin
                        clientCommunication.SendData(Operation.Login, "TRUE", "TRUE");
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
                    
                    // Cart cart = new Cart(new Guid(), new DateTimeOffset(), new DateTimeOffset(), idCustomer );
                    //
                    // List<CartItem> cartItems = new List<CartItem>();
                    //
                    // //To ma być w pętli, tyle razy ile jest wierszy w zamówieniu klienta
                    // CartItem cartItem = new CartItem(new Guid(), cart, new Product(), new int());
                    // cartItems.Add(cartItem);
                    // // to
                    //
                    // cart.Products = cartItems;
                    //
                    // //TODO zapisz transakcje do CSV
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