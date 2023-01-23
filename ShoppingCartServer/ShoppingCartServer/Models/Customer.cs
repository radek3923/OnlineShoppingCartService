namespace ShoppingCartServer.Models;

public class Customer : User
{
    private Guid CartId { get; set; }
    private string FirstName { get; set; } 
    private string LastName { get; set; }

    public Customer(string login, string password, string addressEmail, string phoneNumber, Guid cartId, string firstName, string lastName) : base(login, password, addressEmail, phoneNumber)
    {
        CartId = cartId;
        FirstName = firstName;
        LastName = lastName;
    }
}