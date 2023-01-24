using System;
using System.IO.Pipes;
using ShoppingCartServer.Enums;
using ShoppingCartServer.Models;

class ShopApp
{
    private const string AppName = "Sklep internetowy";
    
    private const string customersPathFile = @"customers.csv";
    // private const string customersPathFile = @"products.csv";
    

    private static List<Customer> _customers;
    
    private static readonly Admin admin = new("admin", "admin", "admin.email.pl", "123456789", AccessLevel.Full);
    

    public static async Task Main()
    {
        _customers = await importCustomers(customersPathFile);
        
        foreach (var customer in _customers)
        {
            Console.WriteLine(customer.Login+", " + customer.Password);
        }
        
        
        var pipeServer = new NamedPipeServerStream("PipeName", PipeDirection.InOut, 2);

        Console.WriteLine("Oczekiwanie na połączenie z klientem");
        //Waiting when client connect to server
        pipeServer.WaitForConnection();
        Console.Clear();
        Console.WriteLine("Połączono z klientem");
        
        State stateConnection = State.Connected;
        
        var reader = new StreamReader(pipeServer);
        var writer = new StreamWriter(pipeServer);
        
        while (State.Connected.Equals(stateConnection))
        {
            var option = int.Parse(reader.ReadLine()!);
            switch (option)
            {
                //Login
                case 1:

                    var login = reader.ReadLine();
                    var password = reader.ReadLine();
                    Console.WriteLine("Login: " + login);
                    Console.WriteLine("Haslo: " + password);

                    if (admin.Login.Equals(login) && admin.Password.Equals(password))
                    {
                        writer.WriteLine("TRUE");// it means user is logged
                        writer.WriteLine("TRUE"); // it means user is admin
                    }
                    else
                    {
                        writer.WriteLine("TRUE");// it means user is logged
                        writer.WriteLine("FALSE"); // it means user is not admin
                        var customer = isCustomerExist(login, password);
                    }
                    Console.WriteLine(_customers.Capacity);

                    writer.Flush();

                    break;
            }
        }

        pipeServer.Close();
    }

    public static Customer isCustomerExist(String login, String password)
    {
        //Search is User exist in Database 
       
        Customer customer = new Customer("login", "haslo", "email", "nrTel", new Guid(), "radek", "potocki");
        return customer;
    }
    
    public static Task<List<Customer>> importCustomers(String customersPathFile) => Task.Run( () =>
    {
        List<Customer> customers = new List<Customer>();
        // try
        // {
            // using StreamReader reader = new StreamReader(customersPathFile);
            // while (true)
            // {
            //     string? line =  reader.ReadLine();
            //     if (line == null)
            //     {
            //         break;
            //     }
            //
            //     string[] split = line.Split(",");
            //     string login = split[0];
            //     string password = split[1];
            //     string addressEmail = split[2];
            //     string phoneNumber = split[3];
            //     Guid cartId = new Guid(split[4]);
            //     string firstName = split[5];
            //     string lastName = split[6];
            //     
            //     var customer = new Customer(login, password, addressEmail, phoneNumber, cartId, firstName, lastName);
            //     customers.Add(customer);
            // }

            foreach (var line in File.ReadLines(customersPathFile))
            {
                string[] split = line.Split(",");
                string login = split[0];
                string password = split[1];
                string addressEmail = split[2];
                string phoneNumber = split[3];
                //Guid cartId = new Guid(split[4]);
                string firstName = split[5];
                string lastName = split[6];
                
                var customer = new Customer(login, password, addressEmail, phoneNumber, new Guid(), firstName, lastName);
                customers.Add(customer);
            }
            return customers;
        // }
        // catch(IOException)
        // {
        //     throw new Exception($"File {customersPathFile} doesnt exist");
        // }
        // return customers;
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