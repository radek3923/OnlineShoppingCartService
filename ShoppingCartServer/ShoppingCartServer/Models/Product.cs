using ShoppingCartServer.Utils;

namespace ShoppingCartServer.Models;

public sealed class Product : Entity
{
    [Order(4)]
    public string Name { get; set; }
    [Order(5)]
    public string NamePlural { get; set; }
    [Order(6)]
    public decimal UnitPrice { get; set; }

    public Product(Guid id, DateTimeOffset createdAt, DateTimeOffset updatedAt, string name, string namePlural, decimal unitPrice) : base(id, createdAt, updatedAt)
    {
        Name = name;
        NamePlural = namePlural;
        UnitPrice = unitPrice;
    }
    
    public override string ToString()
    {
        return string.Format("id: {0}, name: {1}, namePlural: {2}, unitPrice: {3}, createdAt: {4}, updatedAt: {5}", 
            Id, Name, NamePlural, UnitPrice, CreatedAt, UpdatedAt);
    }
}