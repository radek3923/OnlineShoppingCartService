using System.Text.RegularExpressions;
using ShoppingCartServer.Interfaces;
using ShoppingCartServer.Models;

namespace ShoppingCartServer.Utils.Validators;

public class DataValidator : IEmailValidator, ILoginValidator, IPhoneNumberValidator
{

    public bool isDataValid(string login, string email, string phoneNumber, List<Customer> customers)
    {
        bool isDatavalid = true;
        
        if (!isLoginValid(login, customers))
        {
            isDatavalid = false;
        }
        if (!isEmailValid(email))
        {
            isDatavalid = false;
        }
        if (!isPhoneNumberValid(phoneNumber))
        {
            isDatavalid = false;
        }
        
        return isDatavalid;
    }
    
    public bool isEmailValid(string email)
    {
        Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
        Match match = regex.Match(email);
        if (match.Success) return true;
        else
        {
            Console.WriteLine("Podany email nie przechodzi walidacji");
            return false;
        }
    }

    public bool isLoginValid(string login, List<Customer> customers)
    {
        bool isLoginNew = true;
        
        foreach (var customer in customers)
        {
            if (customer.Login == login)
            {
                isLoginNew = false;
            }
        }
        if (isLoginNew)
        {
            return true;
        }
        else if ("admin".Equals(login))
        {
            Console.WriteLine("Login nie może mieć wartość: admin");
            return false;
        }
        else
        {
            Console.WriteLine("Uzytkownik o podanym loginie: {0} już istnieje", login);
            return false;
        }
    }

    public bool isPhoneNumberValid(string phoneNumber)
    {
        if (phoneNumber.Length != 9)
        {
            Console.WriteLine("Numer telefonu ma inną długość niż 9 cyfr");
            return false;
        }
        return true;
    }
}