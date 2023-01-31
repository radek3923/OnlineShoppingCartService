using System.IO.Pipes;
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
    private static int maxDataLoadTime = 60;
    
    private const string customersPathFile = @"customers.csv";
    private const string productsPathFile = @"products.csv";
    private const string shoppingHistoryPathFile = @"shoppingCartsHistory.csv";

    public static async Task Main()
    {
        DataGenerator dataGenerator = new DataGenerator();
        
        CancellationTokenSource cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(maxDataLoadTime));
        
        InputFileManager inputFileManager = new InputFileManager(customersPathFile, productsPathFile, shoppingHistoryPathFile, cts);
        OutputFileManager outputFileManager = new OutputFileManager(customersPathFile, productsPathFile, shoppingHistoryPathFile);

        using (inputFileManager)
        {
            var (importCustomersTask, importProductTask, importShoppingCartsHistoryTask) =
                (inputFileManager.importCustomers(), inputFileManager.importProducts(), inputFileManager.importShoppingCartsHistory());
            try
            {
                _customers = await importCustomersTask;
                _products = await importProductTask;
                _historyShoppingCarts = await importShoppingCartsHistoryTask;
            }
            catch(OperationCanceledException)
            {
                throw new Exception(String.Format("Wczytywanie danych z pliku przekroczyło limit {0}s. Trwa zamknięcie programu.", maxDataLoadTime));
            }
            //probably not required, because this method is executed anyway
            //inputFileManager.Dispose();
        }
        
        // List<CartItem> cartItems = new List<CartItem>();
        // CartItem cartItem1 = new CartItem(new Guid(), new Guid(), 0);
        // CartItem cartItem2 = new CartItem(new Guid(), new Guid(), 0);
        // cartItems.Add(cartItem1);
        // cartItems.Add(cartItem2);
        
        // OutputFileManager.saveObjectToDatabase(new Customer("test1", "test2", "test3", "test4", new Guid(), "test6", "test7"));
        // OutputFileManager.saveObjectToDatabase(new Product(new Guid(), new DateTimeOffset(), new DateTimeOffset(), "2", "2", new decimal()));
        //OutputFileManager.saveCartToDatabase(new Cart(new Guid(), new DateTimeOffset(), new DateTimeOffset(), new Guid(), cartItems ));
         
        // showAllList(_products);
        // Console.WriteLine();
        // showAllList(_customers);
        // Console.WriteLine();
        // showAllList(_historyShoppingCarts);
        // Console.WriteLine();
         
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
                    bool isUserAdmin = admin.Login.Equals(login) && admin.Password.Equals(password);
                    
                    if (loggedCustomer is null && !isUserAdmin)
                    {
                        clientCommunication.SendData(Operation.Login, "FALSE", "FALSE");
                        Console.WriteLine("Nie istnieje uzytkownik o danych login:{0}, password:{1}. Logowanie nieudane", login, password);
                    }
                    else
                    {
                        if (isUserAdmin)
                        {
                            clientCommunication.SendData(Operation.Login, "TRUE", "TRUE");
                        }
                        else
                        {
                            clientCommunication.SendData(Operation.Login, "TRUE", "FALSE");
                        }
                        Console.WriteLine("Uzytkownik {0} zalogował się do serwisu", login);
                        
                        clientCommunication.SendData(Operation.SendingProducts, outputFileManager.ListOfProductsToCsv(_products));
                        Console.WriteLine("Przeslano listę produktów ze sklepu");
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
                        outputFileManager.saveObjectToDatabase(newCustomer);
                        Console.WriteLine("Rejestracja uzytkownika udana.");
                        clientCommunication.SendData(Operation.Register, "TRUE");
                    }
                    else
                    {
                        Console.WriteLine("Rejestracja uzytkownika nieudana.");
                        clientCommunication.SendData(Operation.Register, "FALSE");
                    }
                    break;
                case Operation.Buy:
                    // data 
                    List<CartItem> cartItemsBoughtByCustomer = new List<CartItem>();
                    Cart cart = new Cart(dataGenerator.getNewGuID(), dataGenerator.GetActualDateTimeOffset(), dataGenerator.GetActualDateTimeOffset(), loggedCustomer.Id, cartItemsBoughtByCustomer );
                    for (int i = 1; i < data.Length; i++)
                    {
                        var dataSpilted = data[i].Split("&");
                        CartItem cartItem = new CartItem(dataGenerator.getNewGuID(), Guid.Parse(dataSpilted[0]),
                            int.Parse(dataSpilted[2]));
                        cartItemsBoughtByCustomer.Add(cartItem);
                    }
                    cart.Products = cartItemsBoughtByCustomer;
                    outputFileManager.saveCartToDatabase(cart);
                    Console.WriteLine("Poprawnie zapisano informację o sprzedaży do bazy");
                    break;
                case Operation.ModifyProducts:
                    _products.Clear();
                    for (int i = 1; i < data.Length; i++)
                    {
                        var dataSpilted = data[i].Split(";");

                        _products.Add(new Product(Guid.Parse(dataSpilted[0]), DateTimeOffset.Parse(dataSpilted[1]), DateTimeOffset.Parse(dataSpilted[2]), dataSpilted[3], dataSpilted[4],
                            int.Parse(dataSpilted[5])));


                    }
                    outputFileManager.clearDatabase(productsPathFile);
                    Console.WriteLine(_products.Count);
                    _products.ForEach(p => outputFileManager.saveObjectToDatabase(p));
                    Console.WriteLine("Zmodyfikowano liste produktow w bazie danych");
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