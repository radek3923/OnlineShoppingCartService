namespace ShoppingCartServer.Models;

public class CartItem
{
    public Guid CartItemId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }

    public CartItem(Guid cartItemId, Guid productId, int quantity)
    {
        CartItemId = cartItemId;
        ProductId = productId;
        Quantity = quantity;
    }

    public override string ToString()
    {
        return string.Format("cartItemId: {0}, productId: {1}, quantity: {2} ", 
            CartItemId, ProductId, Quantity);
    }
}