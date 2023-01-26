namespace ShoppingCartServer.Models;

public abstract class User
{
    public string Login { get; set; }
    public string Password{ get; set; }
    protected string AddressEmail { get; set; }
    protected string PhoneNumber { get; set; }
    
    protected User(string login, string password, string addressEmail, string phoneNumber)
    {
        Login = login;
        Password = password;
        AddressEmail = addressEmail;
        PhoneNumber = phoneNumber;
    }
}