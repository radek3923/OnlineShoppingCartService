namespace ShoppingCartServer.Models;

public class CartItem
{
    public Guid CartId { get; set; }
    public Cart Cart { get; set; }

    public Product Product { get; set; }
    public int Quantity { get; set; }
    
}