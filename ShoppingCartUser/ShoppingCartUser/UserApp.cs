using Microsoft.VisualBasic.FileIO;
using System;
using System.IO.Pipes;

class UserApp
{
    static void Main()
    {
        var pipeClient = new NamedPipeClientStream(".", "PipeName", PipeDirection.InOut);
        Console.WriteLine("Oczekiwanie na połączenie z serwerem");
        pipeClient.Connect(); // iniciuje
        Console.Clear();
        
        int dec;

        var reader = new StreamReader(pipeClient);
        var writer = new StreamWriter(pipeClient);
        while (true)
        {
            Console.WriteLine("Wybierz opcje:");
            Console.WriteLine("0 - Logowanie");
            Console.WriteLine("1 - Rejestracja");
            Console.WriteLine("2 - Zakoncz");

            try
            {
                dec = int.Parse(Console.ReadLine()!);
            }
            catch (FormatException)
            {
                dec = -1;
            }

            switch (dec)
            {
                case 0:
                    Console.WriteLine("Podaj login: ");
                    var login = Console.ReadLine();

                    Console.WriteLine("Podaj haslo: ");
                    var haslo = Console.ReadLine();

                    writer.WriteLine("1");
                    writer.WriteLine(login);
                    writer.WriteLine(haslo);
                    writer.Flush(); // wyslanie, zwolnienie bufora

                    // czekanie na odpowedz
                    var response = bool.Parse(reader.ReadLine()!);
                    Console.WriteLine("Received from Parent: " + response);
                    if (response)
                    {
                        Console.WriteLine("Halo");
                    }
                    else
                        throw new Exception();

                    break;
            }
        }
    }

    static void przetwarzanie(string data)
    {
    }
}