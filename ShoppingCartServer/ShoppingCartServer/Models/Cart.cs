namespace ShoppingCartServer.Models;

public sealed class Cart : Entity
{   
    public List<CartItem> Products { get; set; } = new List<CartItem>();

    public Cart(Guid id, DateTimeOffset createdAt, DateTimeOffset updatedAt) : base(id, createdAt, updatedAt)
    {
    }
}