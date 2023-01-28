using System.IO.Pipes;
using ShoppingCartServer.Enums;
using ShoppingCartServer.FileOperations;
using ShoppingCartServer.Models;
using ShoppingCartUser.Communication;

class ShopApp
{
    private const string AppName = "Sklep internetowy";
    
    private static List<Customer> _customers;
    private static List<Product> _products;
    private static readonly Admin admin = new("admin", "admin", "admin.email.pl", "123456789", AccessLevel.Full);


    public static async Task Main()
    {
        FileManager fileManager = new FileManager();
        
        var (importCustomersTask, importProductTask) =
            (fileManager.importCustomers(), fileManager.importProducts());

        _customers = await importCustomersTask;
        _products = await importProductTask;
        
        // showAllList(_products);
        // showAllList(_customers);

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

                    var customer = findCustomerInDatabase(login, password, _customers);

                    if (customer is not null)
                    {
                        //it means user is logged and user is not admin
                        clientCommunication.SendData(Operation.Login, "TRUE", "FALSE");
                        
                    }
                    else if (admin.Login.Equals(login) && admin.Password.Equals(password))
                    {
                        //it means user is logged and user is admin
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
                    Console.WriteLine(data[1]);
                    Console.WriteLine(data[2]);
                    Console.WriteLine(data[3]);
                    Console.WriteLine(data[4]);
                    Console.WriteLine(data[5]);
                    Console.WriteLine(data[6]);

                    //TODO validation data, then create new Customer
                    Customer newCustomer = new Customer(data[1], data[2], data[3], data[4], new Guid(), data[5], data[6]);
                    _customers.Add(newCustomer);
                    //TODO save new Customer to excel
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