using ShoppingCartServer.Utils;

namespace ShoppingCartServer.Models;

public abstract class User
{
    [Order(1)]
    public string Login { get; set; }
    [Order(2)]
    public string Password{ get; set; }
    [Order(3)]
    public string AddressEmail { get; set; }
    [Order(4)]
    public string PhoneNumber { get; set; }
    
    protected User(string login, string password, string addressEmail, string phoneNumber)
    {
        Login = login;
        Password = password;
        AddressEmail = addressEmail;
        PhoneNumber = phoneNumber;
    }
}