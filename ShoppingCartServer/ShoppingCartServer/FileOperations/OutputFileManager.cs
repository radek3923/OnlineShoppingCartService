using ShoppingCartServer.Models;
using ShoppingCartServer.Utils;

namespace ShoppingCartServer.FileOperations;

public sealed class OutputFileManager : FileManager
{
    public OutputFileManager(string customersPathFile, string productsPathFile, string shoppingHistoryPathFile) : base(customersPathFile, productsPathFile, shoppingHistoryPathFile)
    {
    }

    public static string objectToCsv<T>(T obj)
    {
        var properties = from property in typeof(T).GetProperties()
            where Attribute.IsDefined(property, typeof(OrderAttribute))
            orderby ((OrderAttribute)property
                .GetCustomAttributes(typeof(OrderAttribute), false)
                .Single()).Order
            select property;
        
        var objectInCsv = string.Join(s1, properties.Select(p => p.GetValue(obj, null).ToString()));
        return objectInCsv;
    }
    
    public bool saveObjectToDatabase<T>(T obj)
    {
        string filePath = "";
        if (obj is Customer)
        {
            filePath = customersPathFile;
        }
        else if (obj is Product)
        {
            filePath = productsPathFile;
        }
        else
        {
            Console.WriteLine("Nie rozpoznano ścieżki do zapisu obiektu: {0}", obj.GetType());
            return false;
        }
        
        try
        {
            File.AppendAllText(filePath, objectToCsv(obj) + "\n");
            return true;
        }
        catch
        {
            Console.WriteLine("Wystąpił błąd podczas zapisywania obiektu do bazy");
        }
        return false;
    }

    public bool saveCartToDatabase(Cart cart)
    {
        try
        {
            File.AppendAllText(shoppingHistoryPathFile, cartToCsv(cart));
            Console.WriteLine("Poprawnie zapisano obiekt do bazy");
            return true;
        }
        catch
        {
            Console.WriteLine("Wystąpił błąd podczas zapisywania obiektu do bazy");
        }
        return false;
    }

    public static string cartToCsv(Cart cart)
    {
        string objectInCsv = cart.Id + s1 + cart.CreatedAt + s1 + cart.UpdatedAt + s1 + cart.CustomerId + s1;

        foreach (var orderItem in cart.Products)
        {
            objectInCsv += orderItem.CartItemId + s2 + orderItem.ProductId + s2 + orderItem.Quantity;
            objectInCsv += s3;
        }
        objectInCsv = objectInCsv.Remove(objectInCsv.Length-1) + "\n";
        
        return objectInCsv;
    }
    
    public string ListOfProductsToCsv(List<Product> products)
    {
        string productsAsString ="";
        foreach (var product in products)
        {
            productsAsString += objectToCsv(product);
            productsAsString += s2;
        }
        productsAsString = productsAsString.Remove(productsAsString.Length-1);
        
        return productsAsString;
    }

    public void clearDatabase(string pathfile)
    {
        File.WriteAllText(pathfile,string.Empty);
    }
}