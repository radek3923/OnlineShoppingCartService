using Microsoft.VisualBasic.FileIO;
using System;
using System.IO.Pipes;
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
                    //tuple.Item2 means isUserAnAdmin

                    if (tuple.Item1)
                    {
                        if (UserType.Admin.Equals(tuple.Item2))
                        {
                            //show menu for admin 
                            Console.WriteLine("Panel admina");
                        }
                        else
                        {
                            //show menu for casual user
                            Console.WriteLine("Panel klienta");
                        }

                        option = -1;
                    }

                    break;
                case 1:
                    //Registration
                    RegistrationForm(writer, reader);
                    break;
                case 2:
                    option = endTheProgram(pipeClient, writer, reader);
                    break;
                default:
                    Console.WriteLine("Niepoprawna wartość, spróbuj ponownie");
                    break;
            }
        } while (option != -1);


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