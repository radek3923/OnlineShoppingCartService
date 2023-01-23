﻿using Microsoft.VisualBasic.FileIO;
using System;
using System.IO.Pipes;

class ChildProcess
{
    static void Main()
    {
        var pipeClient = new NamedPipeClientStream(".", "PipeName", PipeDirection.InOut);
        pipeClient.Connect(); // iniciuje
        int dec;


        using (var reader = new StreamReader(pipeClient))
        using (var writer = new StreamWriter(pipeClient))
        {
            while (true)
            {
                Console.WriteLine("Wybierz opcje:");
                Console.WriteLine("0 - Logowanie");
                Console.WriteLine("1 - Rejestracja");
                
                try
                {
                    dec = int.Parse(Console.ReadLine()!);
                }
                catch (FormatException)
                {
                    dec = -1;
                }
                
                switch(dec)
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
    }

    static void przetwarzanie(string data)
    {

    }
}