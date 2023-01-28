using ShoppingCartUser.Enums;

namespace ShoppingCartUser.Communication;

public class ServerCommunication
{
    private StreamReader reader;
    private StreamWriter writer;

    public ServerCommunication(StreamReader reader, StreamWriter writer)
    {
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