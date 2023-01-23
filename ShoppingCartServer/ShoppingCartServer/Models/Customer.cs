namespace ShoppingCartServer.Models;

public class Customer : User
{
    private Guid CartId { get; set; }
    private string FirstName { get; set; } 
    private string LastName { get; set; }
}