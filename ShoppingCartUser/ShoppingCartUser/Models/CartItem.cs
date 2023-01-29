namespace ShoppingCartUser.App;

public class CartItem
{
    public Guid CartId { get; set; }
    public string Name { get; set; }
    public string NamePlural { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    
    public CartItem(Guid cartId, string name, string namePlural, decimal unitPrice, int quantity)
    {
        CartId = cartId;
        Name = name;
        NamePlural = namePlural;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }

    public string mergedString(string separator)
    {
        return CartId + separator + Name + separator + Quantity;
    }
}