namespace ShoppingCartUser.App;

public class CartItem
{
    public Guid CartId { get; set; }
    public string Name { get; set; }
    public string NamePlural { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}