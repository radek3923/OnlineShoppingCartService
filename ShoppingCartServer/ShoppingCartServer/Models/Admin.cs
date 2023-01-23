using ShoppingCartServer.Enums;

namespace ShoppingCartServer.Models;

public class Admin : User
{
    private AccessLevel Access { get; set; }

    public Admin(string login, string password, string addressEmail, string phoneNumber, AccessLevel access) : base(login, password, addressEmail, phoneNumber)
    {
        Access = access;
    }
}