namespace ShoppingCartServer.Models;

public class CartItem
{
    public Guid CartItemId { get; set; }
    public Cart Cart { get; set; }

    public Product Product { get; set; }
    public int Quantity { get; set; }

    public CartItem(Guid cartItemId, Cart cart, Product product, int quantity)
    {
        CartItemId = cartItemId;
        Cart = cart;
        Product = product;
        Quantity = quantity;
    }
}