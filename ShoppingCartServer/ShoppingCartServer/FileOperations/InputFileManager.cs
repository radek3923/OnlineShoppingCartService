using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using ShoppingCartServer.Models;

namespace ShoppingCartServer.FileOperations;

public class InputFileManager : FileManager, IDisposable
{
    private CancellationTokenSource cts;
    private bool _disposedValue;
    private SafeHandle _safeHandle = new SafeFileHandle(IntPtr.Zero, true);

    public InputFileManager(string customersPathFile, string productsPathFile, string shoppingHistoryPathFile, CancellationTokenSource cts) : base(customersPathFile, productsPathFile, shoppingHistoryPathFile)
    {
        this.cts = cts;
    }

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
        Console.WriteLine("Zwolnienie zasobów z klasy {0}", this.GetType().Name );
    }

    public Task<List<Customer>> importCustomers() => Task.Run(() =>
    {
        List<Customer> customers = new List<Customer>();
        try
        {
            foreach (var line in File.ReadLines(customersPathFile))
            {
                cts.Token.ThrowIfCancellationRequested();
                string[] split = line.Split(s1);
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
                string[] split = line.Split(s1);
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
                
                string[] split = line.Split(s1);
                
                //split[] is CartId ; CartCreatedAt ; CartUpdatedAt ; CustomerId ; List<CartItem>
                Guid cartId = Guid.Parse(split[0]);
                DateTimeOffset cartCreatedAt = DateTimeOffset.Parse(split[1]);
                DateTimeOffset cartUpdatedAt = DateTimeOffset.Parse(split[2]);
                Guid customerId = Guid.Parse(split[3]);
                
                //split[4] is CartItemId # ProductId # Quantity    $   second CartItem and etc
                string listOfCartItemsAsString = split[4];
                
                string[] cartItemsAsString = listOfCartItemsAsString.Split(s3);

                foreach (var cartItemAsString in cartItemsAsString)
                {
                    string[] items = cartItemAsString.Split(s2);
                    
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
}