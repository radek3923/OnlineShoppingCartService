﻿using System.IO.Pipes;
using ShoppingCartServer.Enums;
using ShoppingCartServer.Models;
using ShoppingCartUser.Communication;

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

        // showAllList(_products);
        // Console.WriteLine();
        // showAllList(_customers);
        // Console.WriteLine();

        var pipeServer = new NamedPipeServerStream("PipeName", PipeDirection.InOut, 2);
        var reader = new StreamReader(pipeServer);
        var writer = new StreamWriter(pipeServer);
        ClientCommunication clientCommunication = new ClientCommunication(reader, writer);
        Console.WriteLine("Oczekiwanie na połączenie z klientem");
        pipeServer.WaitForConnection();
        Console.Clear();
        Console.WriteLine("Połączono z klientem");
        State stateConnection = State.Connected;

        while (State.Connected.Equals(stateConnection))
        {
            string[] data = clientCommunication.ReadData();
            Operation operation = Enum.Parse<Operation>(data[0]);

            switch (operation)
            {
                case Operation.Disconnect:
                    Console.WriteLine("Trwa wyłączanie serwera ");
                    stateConnection = State.Disconnected;
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

                        var products_string = await Task.Run((() => File.ReadAllText(productsPathFile)));
                        // writer.Write(products_string);
                        // writer.Flush();
                        // Console.WriteLine("Przeslano");
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

    public static Task<List<Customer>> importCustomers(String customersPathFile) => Task.Run(() =>
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
                Guid Id = Guid.Parse(split[4]);
                string firstName = split[5];
                string lastName = split[6];

                var customer = new Customer(login, password, addressEmail, phoneNumber, Id, firstName, lastName);
                customers.Add(customer);
            }

            return customers;
        }
        catch (IOException)
        {
            throw new Exception($"File {customersPathFile} doesnt exist");
        }
    });

    public static Task<List<Product>> importProducts(String productsPathFile) => Task.Run(() =>
    {
        List<Product> products = new List<Product>();
        try
        {
            foreach (var line in File.ReadLines(productsPathFile))
            {
                string[] split = line.Split(";");
                Guid id = Guid.Parse(split[0]);
                DateTimeOffset createdAt = DateTimeOffset.Parse(split[1]);
                DateTimeOffset updatedAt = DateTimeOffset.Parse(split[2]);
                string name = split[3];
                string namePlural = split[4];
                decimal unitPrice = decimal.Parse(split[5]);

                var product = new Product(id, createdAt, updatedAt, name, namePlural, unitPrice);
                products.Add(product);
            }

            return products;
        }
        catch (IOException)
        {
            throw new Exception($"File {productsPathFile} doesnt exist");
        }
    });

    public static async Task<List<Customer>> readAllUsers(String userPathFile)
    {
        String usersAsStringFromCsv = await readCsvFile(customersPathFile);

        String[] split = usersAsStringFromCsv.Split(";");

        return _customers;
    }

    public static Task<string> readCsvFile(String pathFile) => Task.Run(() =>
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

    public static void showAllList<T>(List<T> list)
    {
        foreach (var item in list)
        {
            Console.WriteLine(item);
        }
    }
}