using System.IO.Pipes;
using System.Text.RegularExpressions;
using System.Threading.Channels;
using ShoppingCartUser.App;
using ShoppingCartUser.Communication;
using ShoppingCartUser.Enums;

class UserApp
{
    static void Main()
    {
        //Initialize Communication with server
        var pipeClient = new NamedPipeClientStream(".", "PipeName", PipeDirection.InOut);
        var reader = new StreamReader(pipeClient);
        var writer = new StreamWriter(pipeClient);
        ServerCommunication serverCommunication = new ServerCommunication(reader, writer);
        Console.WriteLine("Oczekiwanie na połączenie z serwerem");
        pipeClient.Connect();
        Console.Clear();


        List<CartItem> cartUser = new List<CartItem>();

        Operation operation;

        do
        {
            Console.WriteLine("Wybierz opcje:");
            Console.WriteLine("0 - Logowanie");
            Console.WriteLine("1 - Rejestracja");
            Console.WriteLine("2 - Zakoncz");

            operation = askForOption();
            Console.Clear();

            switch (operation)
            {
                case Operation.Login:
                    UserType userType = getUserType(serverCommunication);

                    if (UserType.Admin.Equals(userType))
                    {
                        Console.WriteLine("Panel admina");
                        
                        //string[] history = serverCommunication.ReadData();
                        //var history = listProducts(data2);
                        // Console.WriteLine("historia" + history[1]);
                        string[] data = serverCommunication.ReadData();
                        var products = listProducts(data);
                        

                        
                        AdminMenu(products ,  serverCommunication);
                    }
                    else if (UserType.Customer.Equals(userType))
                    {
                        Console.WriteLine("Panel klienta");

                        // list of produts sent by server
                        string[] data = serverCommunication.ReadData();
                        var products = listProducts(data);
                        
                        CustomerMenu(products, serverCommunication);
                    }
                    else
                    {
                        Console.WriteLine("Błędne dane, spróbuj ponownie\n");
                    }

                    break;
                case Operation.Register:
                    RegistrationForm(serverCommunication);
                    break;
                case Operation.Disconnect:
                    break;
                default:
                    Console.WriteLine("Niepoprawna wartość, spróbuj ponownie");
                    break;
            }
        } while (operation != Operation.Disconnect);

        endTheProgram(pipeClient, serverCommunication);
    }

    public static UserType getUserType(ServerCommunication serverCommunication)
    {
        Console.WriteLine("Podaj login: ");
        string login = Console.ReadLine();

        Console.WriteLine("Podaj haslo: ");
        string password = Console.ReadLine();

        serverCommunication.SendData(Operation.Login, login, password);
        string[] data = serverCommunication.ReadData();

        bool areLoginDataCorrect = bool.Parse(data[1]);
        bool isUserAdmin = bool.Parse(data[2]);


        if (areLoginDataCorrect)
        {
            if (isUserAdmin) return UserType.Admin;
            else return UserType.Customer;
        }
        else
        {
            return UserType.None;
        }
    }

    public static void RegistrationForm(ServerCommunication serverCommunication)
    {
        Console.WriteLine("Podaj login: ");
        var login = Console.ReadLine();

        Console.WriteLine("Podaj haslo: ");
        var password = Console.ReadLine();

        Console.WriteLine("Podaj adres email: ");
        var addressEmail = Console.ReadLine();

        Console.WriteLine("Podaj numer telefonu: ");
        var phoneNumber = Console.ReadLine();

        Console.WriteLine("Podaj imie: ");
        var firstName = Console.ReadLine();

        Console.WriteLine("Podaj nazwisko: ");
        var lastName = Console.ReadLine();


        if (login != null && password != null && addressEmail != null && phoneNumber != null && firstName != null &&
            lastName != null)
        {
            serverCommunication.SendData(Operation.Register, login, password, addressEmail, phoneNumber, firstName,
                lastName);
        }
        else
        {
            throw new Exception("Error while entering data");
        }

        string[] registerResponse = serverCommunication.ReadData();
        if (bool.Parse(registerResponse[1])) Console.WriteLine("Rejestracja udana");
        else Console.WriteLine("Rejestracja nieudana, dane nie przeszły walidacji");
        
    }

    public static void CustomerMenu(List<CartItem> _productsInShop, ServerCommunication serverCommunication)
    {
        var shoppingCart = new List<CartItem>();
        int option = -1;
        do
        {
            Console.WriteLine("Wybierz opcje:");
            Console.WriteLine("0 - Wyswietl liste dostepnych produktow");
            Console.WriteLine("1 - Dodaj produkt do koszyka");
            Console.WriteLine("2 - Zmien liczbe produktow");
            Console.WriteLine("3 - Wyswietl koszyk");
            Console.WriteLine("4 - Przejdz do kasy");
            Console.WriteLine("5 - Wyloguj");
            option = askForOption2();
            Console.Clear();
            
            switch (option)
            {
                case 0:
                    _productsInShop.ForEach(p => Console.WriteLine(($" {p.Name,-10}:{p.UnitPrice,5} zł ")));
                    break;
                case 1:
                    // wyszukaj produkt
                    Console.WriteLine("Podaj nazwę produktu, ktory dodac do koszyka");
                    string chosenProduct = Console.ReadLine();
                    Console.WriteLine("Ile sztuk produktu dodac do koszyka");
                    
                    int quantityProduct = int.TryParse(Console.ReadLine(), out var myInt) ? myInt : 0;

                    if (quantityProduct <= 0)
                    {
                        Console.WriteLine("Błędna wartość");
                        break;
                    }

                    // zawsze podmienia
                    if (shoppingCart.Where(p => (p.Name == chosenProduct)).Count() >= 1)
                    {
                        foreach (var p in shoppingCart)
                        {
                            if (p.Name == chosenProduct)
                            {
                                p.Quantity += quantityProduct;
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            var wynik = _productsInShop.Where(p => (p.Name == chosenProduct))
                                .Select(p => (p.CartId, p.Name, p.NamePlural, p.UnitPrice)).First();
                            shoppingCart.Add(new CartItem(wynik.Item1,DateTimeOffset.Now, DateTimeOffset.Now,  wynik.Item2, wynik.Item3, wynik.Item4,
                                quantityProduct));
                        }
                        catch
                        {
                            Console.WriteLine("Taki produkt nie wystepuje w sklepie");
                        }
                    }

                    break;
                case 2:
                    Console.WriteLine("Podaj nazwę produktu, ktorego ilosc chcesz zmienic");
                    string chosenProductName = Console.ReadLine();

                    var selectedProducts = shoppingCart.Where(p => p.Name == chosenProductName);
                    if (selectedProducts.Count() != 0)
                    {
                        Console.WriteLine("Jaką liczbę produków umieścić w koszyku");
                        int quantityProduct1 = int.TryParse(Console.ReadLine(), out var myInt1) ? myInt1 : 0;

                        if (quantityProduct1 < 0)
                        {
                            Console.WriteLine("Podano błędną wartość");
                        }
                        else
                        {
                            foreach (var product in selectedProducts)
                            {
                                product.Quantity = quantityProduct1;
                            }    
                        }
                        
                    }
                    else
                    {
                        Console.WriteLine("Nie ma takiego produktu w koszyku");
                    }
                    break;
                case 3:
                    if (shoppingCart.Count() == 0)
                    {
                        Console.WriteLine("Koszyk jest pusty");
                    }
                    else
                    {
                        shoppingCart.RemoveAll(p => p.Quantity == 0);
                        shoppingCart.ForEach(p =>
                            Console.WriteLine(($" {p.Name,-10}:{p.UnitPrice,5} zł : {p.Quantity,5} sztuk  ##  {p.UnitPrice * p.Quantity, 5} zl")));
                    }
                    break;
                case 4:
                    if (shoppingCart.Count() != 0)
                    {
                        var productsCustomer = shoppingCart.Select(p => p.mergedString("&")).ToArray();
                        serverCommunication.SendData(Operation.Buy, productsCustomer);
                        shoppingCart.Clear();
                        Console.WriteLine("Zakupy udane");
                    }
                    else Console.WriteLine("Koszyk jest pusty");
                    break;
                case 5:
                    Console.WriteLine("Wylogowywanie z konta");
                    break;
            }
        } while (option != 5);
    }

    public static void AdminMenu(List<CartItem> _productsInShop,  ServerCommunication serverCommunication)
    {
        List<string> productsToAdd = new List<string>();
        List<string> productsToRemove = new List<string>();
        Console.Clear();
        int option = -1;
        do
        {
            Console.WriteLine("Wybierz opcje:");
            Console.WriteLine("0 - Wyświetl produkty dostepne w sklepie");
            Console.WriteLine("1 - Dodaj nowy produkt");
            Console.WriteLine("2 - Usun produkt");
            Console.WriteLine("3 - Zatwierdz zmiany");
            Console.WriteLine("4 - Wyloguj");
            option = askForOption2();
            Console.Clear();
            
            switch (option)
            {
                case 0:
                    // lista bedzie odswiezona po nasepnym uruchomieniu serwera
                    _productsInShop.ForEach(p => Console.WriteLine(($" {p.Name,-10}:{p.UnitPrice,5} zł ")));
                    
                    break;
                case 1:
                    Console.WriteLine("Podaj nazwę produktu, który chcesz dodac");
                    var productName = Console.ReadLine();
                    if (_productsInShop.Where(p => p.Name == productName).Count() >= 1)
                    {
                        Console.WriteLine("Taki produkt juz istnieje w sklepie");
                    }
                    else
                    {
                        Console.WriteLine("Podaj liczbe mnogą nazwy produktu, który chcesz dodac");
                        var productNamePlural = Console.ReadLine();
                        int productPrice = 0;
                        do
                        {
                            Console.WriteLine("Podaj cenę produktu");
                            productPrice = int.TryParse(Console.ReadLine(), out var myInt) ? myInt : -1;
                            if(productPrice < 0)
                                Console.WriteLine("Wprowadzono nie poprawna wartosc");
                        } while (productPrice < 0);

                        // id generowane przez serwer
                        string mergedProduct = productName + "&" + productNamePlural + "&" + productPrice;
                        _productsInShop.Add(new CartItem(Guid.NewGuid(), DateTimeOffset.Now,  DateTimeOffset.Now, productName, productNamePlural, productPrice,
                            Int32.MaxValue));
                        productsToAdd.Add(mergedProduct);
                    }
                    
                    break;
                case 2:
                    Console.WriteLine("Podaj nazwę produktu, która chcesz usunac");
                    var productNameToRemove = Console.ReadLine();
                    if (_productsInShop.Where(p => p.Name == productNameToRemove).Count() >= 1)
                    {
                        var guidID = _productsInShop.Where(p => p.Name == productNameToRemove)
                            .Select(p => p.CartId).First();

                        _productsInShop.RemoveAll(p => p.Name == productNameToRemove);
       
                    }
                    else
                    {
                        Console.WriteLine("Nie ma takiego produktu w sklepie");
                    }
                    break;
                case 3:
                    Console.WriteLine("Zatwierdz zmiany");
                    var products = _productsInShop.Select(p => p.CartId + ";" + p.CreatedAt + ";" + p.UpdatedAt + ";" + p.Name + ";" + p.NamePlural + ";" + p.UnitPrice ).ToArray();
                    serverCommunication.SendData(Operation.ModifyProducts, products);
                    break;
                
                case 4:
                    Console.WriteLine("Wylogowywanie z konta");
                    break;
            }

        } while (option != 4);
        
    }

    public static Operation askForOption()
    {
        Operation operation = Operation.None;
        string option = Console.ReadLine();

        if ("0".Equals(option)) return Operation.Login;
        if ("1".Equals(option)) return Operation.Register;
        if ("2".Equals(option)) return Operation.Disconnect;
        else return Operation.None;
    }

    public static int askForOption2()
    {
        int option;

        try
        {
            option = int.Parse(Console.ReadLine()!);
        }
        catch (FormatException)
        {
            option = -1;
        }

        return option;
    }

    public static void endTheProgram(NamedPipeClientStream pipeClient, ServerCommunication serverCommunication)
    {
        Console.WriteLine("Trwa wyłączanie programu ");
        serverCommunication.SendData(Operation.Disconnect);
        pipeClient.Close();
    }

    public static List<CartItem> listProducts(string[] lines)
    {
        var ProductList = new List<CartItem>();

        // last elemnt of string produts in "\n" char
        for (var i = 0; i < lines.Length - 1; i++)
        {
            try
            {
                string[] split = lines[i].Split(";");
                Guid Id = Guid.Parse(split[0]); // naprawic
                string Name = split[3];
                string NamePlural = split[4];
                decimal UnitPrice = Decimal.Parse(split[5]);

                ProductList.Add(new CartItem(Id, DateTimeOffset.Now, DateTimeOffset.Now,  Name, NamePlural, UnitPrice, Int32.MaxValue));
            }
            catch
            {
                continue;
            }
        }

        return ProductList;
    }
}