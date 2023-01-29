using ShoppingCartServer.Models;

namespace ShoppingCartServer.Interfaces;

public interface ILoginValidator
{
    bool isLoginValid(string login,  List<Customer> customers);
}