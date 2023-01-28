using ShoppingCartServer.Interfaces;

namespace ShoppingCartServer.Utils.Validators;

public class DataValidator : IEmailValidator, ILoginValidator, IPhoneNumberValidator
{
    public bool isEmailValid()
    {
        throw new NotImplementedException();
    }

    public bool isLoginValid()
    {
        throw new NotImplementedException();
    }

    public bool isPhoneNumberValidatorValid()
    {
        throw new NotImplementedException();
    }
}