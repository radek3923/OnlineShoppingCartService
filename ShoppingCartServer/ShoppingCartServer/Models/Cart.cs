namespace ShoppingCartServer.Models;

public sealed class Cart : Entity
{   
    public List<CartItem> Products { get; set; } = new List<CartItem>();
    
    public Cart()
    {
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}