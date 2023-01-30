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

            switch (operation)
            {
                case Operation.Login:
                    UserType userType = getUserType(serverCommunication);

                    if (UserType.Admin.Equals(userType))
                    {
                        Console.WriteLine("Panel admina");
                        string[] data = serverCommunication.ReadData();
                        var products = listProducts(data);
                        AdminMenu(products, serverCommunication);
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
            switch (option)
            {
                case 0:
                    _productsInShop.ForEach(p => Console.WriteLine(($" {p.Name,-10}:{p.UnitPrice,5} zł ")));
                    break;
                case 1:
                    // wyszukaj produkt
                    Console.WriteLine("Podaj produkt, ktory dodac do koszyka");
                    string chosenProduct = Console.ReadLine();
                    Console.WriteLine("Ile sztuk produktu dodac do koszyka");
                    
                    int quantityProduct = int.TryParse(Console.ReadLine(), out var myInt) ? myInt : 0;
                    Console.WriteLine(quantityProduct);
                    
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
                            shoppingCart.Add(new CartItem(wynik.Item1, wynik.Item2, wynik.Item3, wynik.Item4,
                                quantityProduct));
                        }
                        catch
                        {
                            Console.WriteLine("Taki produkt nie wystepuje w sklepie");
                        }
                    }

                    break;
                case 2:
                    Console.WriteLine("Podaj produkt, ktorego ilosc zmienic");
                    string chosenProduct1 = Console.ReadLine();

                    if (shoppingCart.Where(p => p.Name == chosenProduct1).Count() == 1)
                    {
                        Console.WriteLine("Ile sztuk produktu w koszuku");
                        int quantityProduct1 = int.TryParse(Console.ReadLine(), out var myInt1) ? myInt1 : 0;

                        if (quantityProduct1 < 0)
                        {
                            Console.WriteLine("Podano błędną wartość");
                        }
                        else
                        {
                            foreach (var product in shoppingCart)
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
                    var productsCustomer = shoppingCart.Select(p => p.mergedString("&")).ToArray();
                    serverCommunication.SendData(Operation.Buy, productsCustomer);
                    shoppingCart.Clear();
                    break;
                case 5:
                    Console.WriteLine("Wylogowywanie z konta");
                    break;
            }
        } while (option != 5);
    }

    public static void AdminMenu(List<CartItem> _productsInShop, ServerCommunication serverCommunication)
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
            Console.WriteLine("3 - Zaktualizuj zmiany");
            Console.WriteLine("4 - Wyloguj");

            option = askForOption2();
            switch (option)
            {
                case 0:
                    // lista bedzie odswiezona po nasepnym uruchomieniu serwera
                    _productsInShop.ForEach(p => Console.WriteLine(($" {p.Name,-10}:{p.UnitPrice,5} zł ")));
                    break;
                case 1:
                    Console.WriteLine("Podaj nazwę produktu, który chcesz dodac");
                    var productName = Console.ReadLine();
                    if (_productsInShop.Where(p => p.Name == productName).Count() == 1)
                    {
                        Console.WriteLine("Taki produkt juz istnieje w sklepie");
                    }
                    else if (productsToAdd.Where(p => p.Split('&')[0] == productName).Count() >= 1)
                    {
                        // kluczem gownym jest tylko name a nie name+pluralName
                        Console.WriteLine("Produkt juz zostal dodany do koszyka");
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
                        productsToAdd.Add(mergedProduct);
                    }
                    
                    break;
                case 2:
                    Console.WriteLine("Podaj nazwę produktu, która chcesz usunac");
                    var productNameToRemove = Console.ReadLine();
                    if (productsToRemove.Where(p => p.Split('&')[1] == productNameToRemove).Count() >= 1)
                    {
                        // kluczem gownym jest tylko name a nie name+pluralName
                        Console.WriteLine("Produkt juz zostal ustawiony do usniecia ");
                    }
                    else if (_productsInShop.Where(p => p.Name == productNameToRemove).Count() >= 1)
                    {
                        // u nas nazwa jest kluczem glownym
                        var guidID = _productsInShop.Where(p => p.Name == productNameToRemove)
                            .Select(p => p.CartId).First();
                        productsToRemove.Add((guidID).ToString() + "&" + productNameToRemove);
                        Console.WriteLine("Produkt zostal dodany do listy produktow do usuniecia");
                    }
                    else
                    {
                        Console.WriteLine("Nie ma takiego produktu w sklepie");
                    }
                    
                    break;
                case 3:
                    // najpierw usuwamy produkt
                    if (productsToRemove.Count != 0)
                    {
                        Console.WriteLine("Produktu do usuniecia :");
                        productsToRemove.ForEach( p => Console.WriteLine(Regex.Replace(p, "&", " : ")));
                        serverCommunication.SendData(Operation.RemoveProducts, productsToRemove.ToArray());
                        productsToRemove.Clear();
                    }
                    if (productsToAdd.Count != 0)
                    {
                        Console.WriteLine("Produkty do dodania");
                        productsToAdd.ForEach( p => Console.WriteLine(Regex.Replace(p, "&", " : ")));
                        serverCommunication.SendData(Operation.AddProducts, productsToAdd.ToArray());
                        productsToAdd.Clear();
                    }
                    if (productsToAdd.Count == 0 && productsToRemove.Count == 0)
                    {
                        Console.WriteLine("Brak produktow do dodania lub do usuniecia");
                    }
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

                ProductList.Add(new CartItem(Id, Name, NamePlural, UnitPrice, Int32.MaxValue));
            }
            catch
            {
                continue;
            }
        }

        return ProductList;
    }
}