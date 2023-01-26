using System.IO.Pipes;
using ShoppingCartServer.Enums;
using ShoppingCartServer.Models;

class ShopApp
{
    private const string AppName = "Sklep internetowy";
    
    private const string customersPathFile = @"customers.csv";
    private const string productsPathFile = @"products.csv";
    

    private static List<Customer> _customers;
    private static List<Product> _products;
    
    private static readonly Admin admin = new("admin", "admin", "admin.email.pl", "123456789", AccessLevel.Full);
    

    public static async Task Main()
    {
        var (importCustomersTask, importProductTask) =
            (importCustomers(customersPathFile), importProducts(productsPathFile));
        
        _customers = await importCustomersTask;
        _products = await importProductTask;

        var pipeServer = new NamedPipeServerStream("PipeName", PipeDirection.InOut, 2);

        Console.WriteLine("Oczekiwanie na połączenie z klientem");
        pipeServer.WaitForConnection();
        Console.Clear();
        Console.WriteLine("Połączono z klientem");
        
        foreach (var customer in _customers)
        {
            Console.WriteLine(customer.ToString());
        }
        
        State stateConnection = State.Connected;
        
        var reader = new StreamReader(pipeServer);
        var writer = new StreamWriter(pipeServer);
        
        while (State.Connected.Equals(stateConnection))
        {
            var option = int.Parse(reader.ReadLine()!);
            switch (option)
            {
                //Disconect
                case -1:
                    Console.WriteLine("testttt ");
                    stateConnection = State.Disconnected;
                    break;
                //Login
                case 1:

                    var login = reader.ReadLine();
                    var password = reader.ReadLine();
                    Console.WriteLine("Login: " + login);
                    Console.WriteLine("Haslo: " + password);

                    var customer = findCustomerInDatabase(login, password);
                    
                    if(customer is not null)
                    {
                        if (admin.Login.Equals(login) && admin.Password.Equals(password))
                        {
                            writer.WriteLine("TRUE");// it means user is logged
                            writer.WriteLine("TRUE"); // it means user is admin
                        }
                        else
                        {
                            writer.WriteLine("TRUE");// it means user is logged
                            writer.WriteLine("FALSE"); // it means user is not admin
                        }
                    }
                    else
                    {
                        writer.WriteLine("FALSE");// it means user not found in database
                        writer.WriteLine("FALSE"); // it means user is not admin
                    }
                    writer.Flush();
                    break;
                //Register
                case 2:
                    break;
            }
        }
        pipeServer.Close();
    }

    public static Customer findCustomerInDatabase(String login, String password)
    {
        //Check if Customer exists in Database 
       
        Customer customer = new Customer("login", "haslo", "email", "nrTel", new Guid(), "radek", "potocki");
        return customer;
    }
    
    public static Task<List<Customer>> importCustomers(String customersPathFile) => Task.Run( () =>
    {
        List<Customer> customers = new List<Customer>();
        try
        {
            foreach (var line in File.ReadLines(customersPathFile))
            {
                string[] split = line.Split(",");
                string login = split[0];
                string password = split[1];
                string addressEmail = split[2];
                string phoneNumber = split[3];
                //Guid Id = new Guid(split[4]);
                string firstName = split[5];
                string lastName = split[6];
                
                var customer = new Customer(login, password, addressEmail, phoneNumber, new Guid(), firstName, lastName);
                customers.Add(customer);
            }
            return customers;
        }
        catch(IOException)
        {
            throw new Exception($"File {customersPathFile} doesnt exist");
        }
    });
    
    public static Task<List<Product>> importProducts(String productsPathFile) => Task.Run( () =>
    {
        List<Product> products = new List<Product>();
        try
        {
            foreach (var line in File.ReadLines(productsPathFile))
            {
                string[] split = line.Split(",");
                // Guid id = split[0];
                // DateTimeOffset createdAt = split[1];
                // DateTimeOffset updatedAt = split[2];
                // string name = split[3];
                // string namePlural = split[5];
                // decimal unitPrice = split[6];
                
                var product = new Product(new Guid(),new DateTimeOffset(), new DateTimeOffset(), "", "", new decimal());
                products.Add(product);
            }
            return products;
        }
        catch(IOException)
        {
            throw new Exception($"File {productsPathFile} doesnt exist");
        }
    });
    
    public static async Task<List<Customer>> readAllUsers(String userPathFile)
    {
        String usersAsStringFromCsv = await readCsvFile(customersPathFile);
    
        String[] split = usersAsStringFromCsv.Split(";");
        
        //List<User> _users = usersAsStringFromCsv.
    
        return _customers;
    }
    
    public static Task<string> readCsvFile(String pathFile) => Task.Run( () =>
    {
        Thread.Sleep(1000);
        try
        {
            return File.ReadAllText(pathFile);
        }
        catch (IOException e)
        {
            Console.WriteLine("File {0} doesnt exist. Error message: {1}", pathFile, e.Source);
            return "";
        }
    });
}