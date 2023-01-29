namespace ShoppingCartServer.Models;

public class CartItem
{
    public Guid CartId { get; set; }
    public Guid CartItemId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }

    public CartItem(Guid cartId, Guid cartItemId, Guid productId, int quantity)
    {
        CartId = cartId;
        CartItemId = cartItemId;
        ProductId = productId;
        Quantity = quantity;
    }
}