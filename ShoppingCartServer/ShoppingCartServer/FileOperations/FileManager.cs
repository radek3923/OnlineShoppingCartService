using ShoppingCartServer.Models;

namespace ShoppingCartServer.FileOperations;

public class FileManager
{
    private const string customersPathFile = @"customers.csv";
    private const string productsPathFile = @"products.csv";
    
    
    public Task<List<Customer>> importCustomers() => Task.Run(() =>
    {
        List<Customer> customers = new List<Customer>();
        try
        {
            foreach (var line in File.ReadLines(customersPathFile))
            {
                string[] split = line.Split(",");
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
                string[] split = line.Split(";");
                Guid id = Guid.Parse(split[0]);
                DateTimeOffset createdAt = DateTimeOffset.Parse(split[1]);
                DateTimeOffset updatedAt = DateTimeOffset.Parse(split[2]);
                string name = split[3];
                string namePlural = split[4];
                decimal unitPrice = decimal.Parse(split[5]);

                var product = new Product(id, createdAt, updatedAt, name, namePlural, unitPrice);
                products.Add(product);
            }
            return products;
        }
        catch (IOException)
        {
            throw new Exception($"File {productsPathFile} doesnt exist");
        }
    });
}