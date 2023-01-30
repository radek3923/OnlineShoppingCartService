using System.Text;
using ShoppingCartServer.Enums;

namespace ShoppingCartUser.Communication;

public class ClientCommunication
{
    public State State { get; set; }
    private StreamReader reader;
    private StreamWriter writer;

    public ClientCommunication(State state, StreamReader reader, StreamWriter writer)
    {
        State = state;
        this.reader = reader;
        this.writer = writer;
    }

    public void SendData(Operation operation, params string[] args)
    {
        string data = operation+ "#";
        foreach (string s in args)
        {
            data += s + "#";
        }

        writer.WriteLine(data);
        writer.Flush();
    }

    public string[] ReadData()
    {
        try
        {
            string dataLine = reader.ReadLine();
            String[] data = dataLine.Split('#');
            return data;
        }
        catch
        {
            throw new Exception("Error while loading data ");
        }
    }
}