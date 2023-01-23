using ShoppingCartServer.Enums;

namespace ShoppingCartServer.Models;

public class Admin : User
{
    private AccessLevel Access { get; set; }
}