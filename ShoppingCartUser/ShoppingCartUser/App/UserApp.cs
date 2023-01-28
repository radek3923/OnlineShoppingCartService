﻿using Microsoft.VisualBasic.FileIO;
using System;
using System.IO.Pipes;
using System.Threading.Channels;
using ShoppingCartUser;
using ShoppingCartUser.App;
using ShoppingCartUser.Communication;
using ShoppingCartUser.Enums;

class UserApp
{
    static void Main()
    {
        //Initialize Communication with server
        var pipeClient = new NamedPipeClientStream(".", "PipeName", PipeDirection.InOut);
        var reader = new StreamReader(pipeClient);
        var writer = new StreamWriter(pipeClient);
        ServerCommunication serverCommunication = new ServerCommunication(reader, writer);
        Console.WriteLine("Oczekiwanie na połączenie z serwerem");
        pipeClient.Connect();
        Console.Clear();


        List<CartItem> cartUser = new List<CartItem>();

        Operation option;

        do
        {
            Console.WriteLine("Wybierz opcje:");
            Console.WriteLine("0 - Logowanie");
            Console.WriteLine("1 - Rejestracja");
            Console.WriteLine("2 - Zakoncz");

            option = askForOption();

            switch (option)
            {
                case Operation.Login:
                    var tuple = isUserLogged(serverCommunication);
                    //tuple.Item1 means isUserExist in database
                    //tuple.Item2 means userType (customer or admin)

                    if (tuple.Item1 && UserType.Admin.Equals(tuple.Item2))
                    {
                        Console.WriteLine("Panel admina");
                        //AdminMenu();
                    }
                    else if (tuple.Item1)
                    {
                        //show menu for casual user
                        Console.WriteLine("Panel klienta");
                        Console.WriteLine("Otrzymano ");

                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            Console.WriteLine(line);
                        }
                    }

                    break;
                case Operation.Register:
                    RegistrationForm(serverCommunication);
                    break;
                case Operation.Disconnect:
                    break;
                default:
                    Console.WriteLine("Niepoprawna wartość, spróbuj ponownie");
                    break;
            }
        } while (option != Operation.Disconnect);

        endTheProgram(pipeClient, serverCommunication);
    }

    public static (bool, UserType) isUserLogged(ServerCommunication serverCommunication)
    {
        Console.WriteLine("Podaj login: ");
        string login = Console.ReadLine();

        Console.WriteLine("Podaj haslo: ");
        string password = Console.ReadLine();

        serverCommunication.SendData(Operation.Login, login, password);

        //waiting for response from server
        string[] data = serverCommunication.ReadData();

        bool areLoginDataCorrect = bool.Parse(data[1]);
        bool isUserAdmin = bool.Parse(data[2]);


        Console.WriteLine("areLoginDataCorrect: " + areLoginDataCorrect + ", isUserAdmin: " + isUserAdmin);

        if (areLoginDataCorrect)
        {
            if (isUserAdmin) return (true, UserType.Admin);
            else return (true, UserType.Customer);
        }
        else
        {
            Console.WriteLine("Błędne dane, spróbuj ponownie\n");
            return (false, UserType.None);
        }
    }

    public static void RegistrationForm(ServerCommunication serverCommunication)
    {
        Console.WriteLine("Podaj login: ");
        var login = Console.ReadLine();

        Console.WriteLine("Podaj haslo: ");
        var password = Console.ReadLine();

        Console.WriteLine("Podaj adres email: ");
        var addressEmail = Console.ReadLine();

        Console.WriteLine("Podaj numer telefonu: ");
        var phoneNumber = Console.ReadLine();

        Console.WriteLine("Podaj imie: ");
        var firstName = Console.ReadLine();

        Console.WriteLine("Podaj nazwisko: ");
        var lastName = Console.ReadLine();


        if (login != null && password != null && addressEmail != null && phoneNumber != null && firstName != null &&
            lastName != null)
        {
            serverCommunication.SendData(Operation.Register, login, password, addressEmail, phoneNumber, firstName,
                lastName);
        }
        else
        {
            throw new Exception("Error while entering data");
        }
            

    }

    public static void CustomerMenu()
    {
        Console.Clear();
        Console.WriteLine("Wybierz opcje:");
        Console.WriteLine("0 - Kup produkt");
        Console.WriteLine("1 - Wyświetl produkt");
        Console.WriteLine("2 - Wyloguj");
    }

    public static void AdminMenu()
    {
        Console.Clear();
        Console.WriteLine("Wybierz opcje:");
        Console.WriteLine("0 - Dodaj nowy produkt");
        Console.WriteLine("1 - Modyfikuj produkt");
        Console.WriteLine("2 - Wyloguj");
    }

    public static Operation askForOption()
    {
        Operation operation = Operation.None;
        string option = Console.ReadLine();

        if ("0".Equals(option)) return Operation.Login;
        if ("1".Equals(option)) return Operation.Register;
        if ("2".Equals(option)) return Operation.Disconnect;
        else return Operation.None;
    }

    public static void endTheProgram(NamedPipeClientStream pipeClient, ServerCommunication serverCommunication)
    {
        Console.WriteLine("Trwa wyłączanie programu ");
        serverCommunication.SendData(Operation.Disconnect);
        pipeClient.Close();
    }

    // public void List<string> listProducts(string produts)
    // {
    //         produts.Split('#')
    //         .Where(x => !string.IsNullOrEmpty(x))
    //         .Select(x => x.Split(';'))
    //         .Select(x => ($" {x[3], -10}:{ x[5], 5} "))
    //         .ToList().ForEach(p => Console.WriteLine(p));
    // }
}