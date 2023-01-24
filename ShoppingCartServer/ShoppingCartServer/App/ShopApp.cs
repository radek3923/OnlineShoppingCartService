using System;
using System.IO.Pipes;
using ShoppingCartServer.Enums;
using ShoppingCartServer.Models;

class ShopApp
{
    private const string AppName = "Sklep internetowy";
    
    private const string usersPathFile = "users.csv";
    private const string productsPathFile = "products.csv";
    
    private static List<User> _users = await readAllUsers(usersPathFile);
    //private static List<Product> _products = await readAllProducts(productsPathFile);
    
    private static readonly Admin admin = new("admin", "admin", "admin.email.pl", "123456789", AccessLevel.Full);
    

    static void Main()
    {
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

    public static async Task<List<User>> readAllUsers(String userPathFile)
    {
        String usersAsStringFromCsv = await readCsvFile(usersPathFile);
        
        List<User> _users = usersAsStringFromCsv.

        return _users;
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