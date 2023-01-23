using System;
using System.IO.Pipes;

class ParentProcess
{
    static void Main()
    {
        var pipeServer = new NamedPipeServerStream("PipeName", PipeDirection.InOut, 3);
        pipeServer.WaitForConnection(); // czeka

        using (var reader = new StreamReader(pipeServer))
        using (var writer = new StreamWriter(pipeServer))
        {
            while (true)
            {
                var option = int.Parse(reader.ReadLine()!);
                switch(option)
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
        }
    }
}