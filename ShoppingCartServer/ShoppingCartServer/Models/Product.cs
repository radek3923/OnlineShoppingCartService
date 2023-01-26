namespace ShoppingCartServer.Models;

public sealed class Product : Entity
{
    public string Name { get; set; }
    public string NamePlural { get; set; }
    public decimal UnitPrice { get; set; }

    public Product(Guid id, DateTimeOffset createdAt, DateTimeOffset updatedAt, string name, string namePlural, decimal unitPrice) : base(id, createdAt, updatedAt)
    {
        Name = name;
        NamePlural = namePlural;
        UnitPrice = unitPrice;
    }
}