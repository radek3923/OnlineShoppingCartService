using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using ShoppingCartServer.Models;
using ShoppingCartServer.Utils;

namespace ShoppingCartServer.FileOperations;

public class FileManager : IDisposable
{
    private const string customersPathFile = @"customers.csv";
    private const string productsPathFile = @"products.csv";
    private const string shoppingHistoryPathFile = @"shoppingCartsHistory.csv";
    private CancellationTokenSource cts;
    private bool _disposedValue;
    private SafeHandle _safeHandle = new SafeFileHandle(IntPtr.Zero, true);
    
    public void Dispose() => Dispose(true);
    
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _safeHandle.Dispose();
            }

            _disposedValue = true;
        }
        //Debug log
        //Console.WriteLine("test xd");
    }
    
    public FileManager(CancellationTokenSource cts)
    {
        this.cts = cts;
    }

    //separators
    private const string s1 = ";";
    private const string s2 = "#";
    private const string s3 = "$";

    public Task<List<Customer>> importCustomers() => Task.Run(() =>
    {
        List<Customer> customers = new List<Customer>();
        try
        {
            foreach (var line in File.ReadLines(customersPathFile))
            {
                cts.Token.ThrowIfCancellationRequested();
                string[] split = line.Split(";");
                try
                {
                    string login = split[0];
                    string password = split[1];
                    string addressEmail = split[2];
                    string phoneNumber = split[3];
                    Guid Id = Guid.Parse(split[4]);
                    string firstName = split[5];
                    string lastName = split[6];

                    var customer = new Customer(login, password, addressEmail, phoneNumber, Id, firstName, lastName);
                    customers.Add(customer);
                }
                catch
                {
                    Console.WriteLine("Nie udało się zaimportować obiektu o wartości: {0}", line);
                    break;
                }
            }

            return customers;
        }
        catch (IOException)
        {
            throw new Exception($"File {customersPathFile} doesnt exist");
        }
    });

    public Task<List<Product>> importProducts() => Task.Run(() =>
    {
        List<Product> products = new List<Product>();
        try
        {
            foreach (var line in File.ReadLines(productsPathFile))
            {
                cts.Token.ThrowIfCancellationRequested();
                string[] split = line.Split(";");
                try
                {
                    Guid id = Guid.Parse(split[0]);
                    DateTimeOffset createdAt = DateTimeOffset.Parse(split[1]);
                    DateTimeOffset updatedAt = DateTimeOffset.Parse(split[2]);
                    string name = split[3];
                    string namePlural = split[4];
                    decimal unitPrice = decimal.Parse(split[5]);

                    var product = new Product(id, createdAt, updatedAt, name, namePlural, unitPrice);
                    products.Add(product);
                }
                catch
                {
                    Console.WriteLine("Nie udało się zaimportować obiektu o wartości: {0}", line);
                    break;
                }
                
            }

            return products;
        }
        catch (IOException)
        {
            throw new Exception($"File {productsPathFile} doesnt exist");
        }
    });
    
    public Task<List<Cart>> importShoppingCartsHistory() => Task.Run(() =>
    {
        List<Cart> carts = new List<Cart>();
        try
        {
            foreach (var line in File.ReadLines(shoppingHistoryPathFile))
            {
                cts.Token.ThrowIfCancellationRequested();
                List<CartItem> cartItems = new List<CartItem>();
                
                string[] split = line.Split(";");
                
                //split[] is CartId ; CartCreatedAt ; CartUpdatedAt ; CustomerId ; List<CartItem>
                Guid cartId = Guid.Parse(split[0]);
                DateTimeOffset cartCreatedAt = DateTimeOffset.Parse(split[1]);
                DateTimeOffset cartUpdatedAt = DateTimeOffset.Parse(split[2]);
                Guid customerId = Guid.Parse(split[3]);
                
                //split[4] is CartItemId # ProductId # Quantity    $   second CartItem and etc
                string listOfCartItemsAsString = split[4];
                
                string[] cartItemsAsString = listOfCartItemsAsString.Split("$");

                foreach (var cartItemAsString in cartItemsAsString)
                {
                    string[] items = cartItemAsString.Split("#");
                    
                    Guid cartItemId = Guid.Parse(items[0]);
                    Guid productId = Guid.Parse(items[1]);
                    int quantity = int.Parse(items[2]);
            
                    CartItem cartItem = new CartItem(cartItemId, productId, quantity);
                    cartItems.Add(cartItem);
                }

                var cart = new Cart(cartId, cartCreatedAt, cartUpdatedAt , customerId, cartItems );
                carts.Add(cart);
            }

            return carts;
        }
        catch (IOException)
        {
            throw new Exception($"File {shoppingHistoryPathFile} doesnt exist");
        }
    });
    
    public static string objectToCsv<T>(T obj)
    {
        var properties = from property in typeof(T).GetProperties()
            where Attribute.IsDefined(property, typeof(OrderAttribute))
            orderby ((OrderAttribute)property
                .GetCustomAttributes(typeof(OrderAttribute), false)
                .Single()).Order
            select property;
        
        var objectInCsv = string.Join(";", properties.Select(p => p.GetValue(obj, null).ToString()));
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
            Console.WriteLine("Poprawnie zapisano obiekt do bazy");
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
}