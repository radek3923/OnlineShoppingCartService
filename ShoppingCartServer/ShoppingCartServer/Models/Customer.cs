using ShoppingCartServer.Utils;

namespace ShoppingCartServer.Models;

public class Customer : User
{
    [Order(5)]
    public Guid Id { get; set; }
    [Order(6)]
    public string FirstName { get; set; } 
    [Order(7)]
    public string LastName { get; set; }

    public Customer(string login, string password, string addressEmail, string phoneNumber, Guid id, string firstName, string lastName) : base(login, password, addressEmail, phoneNumber)
    {
        Id = id;
        FirstName = firstName;
        LastName = lastName;
    }

    public override string ToString()
    {
        return string.Format("Login: {0}, Password: {1}, addressEmail: {2}, phoneNumber: {3}, Id: {4}, firstName: {5}, lastName: {6}", 
            Login, Password, AddressEmail, PhoneNumber, Id, FirstName, LastName );
    }
}