using System;
using System.IO.Pipes;
using ShoppingCartServer.Enums;
using ShoppingCartServer.Models;

class ShopApp
{
    private const string AppName = "Sklep internetowy";

    static void Main()
    {
        Admin admin = new("ad\\min", "admin", "admin.email.pl", "123456789", AccessLevel.Full);


        var pipeServer = new NamedPipeServerStream("PipeName", PipeDirection.InOut, 2);

        Console.WriteLine("Oczekiwanie na połączenie z klientem");
        //Waiting when client connect to server
        pipeServer.WaitForConnection();
        Console.Clear();
        
        State stateConnection = State.Connected;
        
        var reader = new StreamReader(pipeServer);
        var writer = new StreamWriter(pipeServer);
        
        while (State.Connected.Equals(stateConnection))
        {
            var option = int.Parse(reader.ReadLine()!);
            switch (option)
            {
                case 1:
                    var login = reader.ReadLine();
                    var haslo = reader.ReadLine();
                    Console.WriteLine("Login: " + login);
                    Console.WriteLine("Haslo: " + haslo);

                    writer.WriteLine("TRUE");
                    writer.Flush();

                    break;
            }
        }

        pipeServer.Close();
    }
}