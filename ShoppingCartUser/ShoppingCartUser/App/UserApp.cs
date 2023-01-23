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

        int option = 1;

        var reader = new StreamReader(pipeClient);
        var writer = new StreamWriter(pipeClient);

        while (option != -1)
        {
            Console.WriteLine("Wybierz opcje:");
            Console.WriteLine("0 - Logowanie");
            Console.WriteLine("1 - Rejestracja");
            Console.WriteLine("2 - Zakoncz");

            try
            {
                option = int.Parse(Console.ReadLine()!);
            }
            catch (FormatException)
            {
                option = -1;
            }

            switch (option)
            {
                case -1:
                    Console.WriteLine("Trwa wyłączanie programu ");
                    writer.WriteLine("-1");
                    pipeClient.Close();
                    break;
                case 0:
                    var tuple= isUserIsLogged(writer, reader);
                    
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
                    RegistrationForm(writer,reader);
                    break;
            }
        }

        
    }
    
    public static (bool, UserType) isUserIsLogged(StreamWriter writer, StreamReader reader)
    {
        Console.WriteLine("Podaj login: ");
        var login = Console.ReadLine();

        Console.WriteLine("Podaj haslo: ");
        var haslo = Console.ReadLine();
                    
        
        writer.WriteLine("1");
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
            Console.WriteLine("Błędne dane, spróbuj ponownie");
            return (false, UserType.None);
        }
    }
    public static void RegistrationForm(StreamWriter writer, StreamReader reader)
    {
        
    }
    
    
}