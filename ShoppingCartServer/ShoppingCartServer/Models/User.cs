namespace ShoppingCartServer.Models;

public abstract class User
{
    private string Login { get; set; }
    private string Password{ get; set; }
    private string AddressEmail { get; set; }
    private string PhoneNumber { get; set; }
}