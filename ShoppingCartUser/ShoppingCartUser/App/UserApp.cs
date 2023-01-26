using Microsoft.VisualBasic.FileIO;
using System;
using System.IO.Pipes;
using ShoppingCartUser.App;
using ShoppingCartUser.Enums;

class UserApp
{
    static void Main()
    {
        var pipeClient = new NamedPipeClientStream(".", "PipeName", PipeDirection.InOut);
        Console.WriteLine("Oczekiwanie na połączenie z serwerem");
        pipeClient.Connect(); // iniciuje
        Console.Clear();
        
        var reader = new StreamReader(pipeClient);
        var writer = new StreamWriter(pipeClient);

        List<CartItem> cartUser = new List<CartItem>();

        int option;
        
        do
        {
            Console.WriteLine("Wybierz opcje:");
            Console.WriteLine("0 - Logowanie");
            Console.WriteLine("1 - Rejestracja");
            Console.WriteLine("2 - Zakoncz");

            option = askForOption();

            switch (option)
            {
                case 0:
                    var tuple = isUserLogged(writer, reader);
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
                    }
                    break;
                case 1:
                    //Registration
                    RegistrationForm(writer, reader);
                    break;
                case 2:
                    option = -1;
                    break;
                default:
                    Console.WriteLine("Niepoprawna wartość, spróbuj ponownie");
                    break;
            }
        } while (option != -1);

        endTheProgram(pipeClient, writer, reader);

    }
    
    public static (bool, UserType) isUserLogged(StreamWriter writer, StreamReader reader)
    {
        Console.WriteLine("Podaj login: ");
        var login = Console.ReadLine();

        Console.WriteLine("Podaj haslo: ");
        var haslo = Console.ReadLine();
                    
        
        writer.WriteLine("1"); // 1 means user wants to login to server
        writer.WriteLine(login);
        writer.WriteLine(haslo);
        writer.Flush(); // wyslanie, zwolnienie bufora
                    
        //waiting for response from user
        var areLoginDataCorrect = bool.Parse(reader.ReadLine()!);
        var isUserAdmin = bool.Parse(reader.ReadLine()!);
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
    public static void RegistrationForm(StreamWriter writer, StreamReader reader)
    {
        
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

    public static int askForOption()
    {
        int option;
        
        try
        {
            option = int.Parse(Console.ReadLine()!);
        }
        catch (FormatException)
        {
            option = -1;
        }
        return option;
    }

    public static int endTheProgram(NamedPipeClientStream pipeClient,StreamWriter writer, StreamReader reader)
    {
        Console.WriteLine("Trwa wyłączanie programu ");
        writer.WriteLine("-1"); //means user wants to disconect from the server
        writer.Flush();
        pipeClient.Close();

        return -1;
    }
    
}