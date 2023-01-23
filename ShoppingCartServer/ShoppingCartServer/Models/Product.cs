namespace ShoppingCartServer.Models;

public sealed class Product : Entity
{
    public string Name { get; set; }
    public string NamePlural { get; set; }
    public decimal UnitPrice { get; set; }  
}