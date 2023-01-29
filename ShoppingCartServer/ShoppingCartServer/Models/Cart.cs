namespace ShoppingCartServer.Models;

public sealed class Cart : Entity
{   
    public List<CartItem> Products { get; set; } = new List<CartItem>();
    public Customer Customer { get; set; }
    
    public Cart(Guid id, DateTimeOffset createdAt, DateTimeOffset updatedAt, Customer customer) : base(id, createdAt, updatedAt)
    {
        Customer = customer;
    }
}