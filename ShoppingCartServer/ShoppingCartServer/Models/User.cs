namespace ShoppingCartServer.Models;

public abstract class User
{
    private string Login { get; set; }
    private string Password{ get; set; }
    private string AddressEmail { get; set; }
    private string PhoneNumber { get; set; }

    protected User(string login, string password, string addressEmail, string phoneNumber)
    {
        Login = login;
        Password = password;
        AddressEmail = addressEmail;
        PhoneNumber = phoneNumber;
    }
}