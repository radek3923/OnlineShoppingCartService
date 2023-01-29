namespace ShoppingCartServer.Models;

public sealed class Cart : Entity
{   
    public Guid CustomerId { get; set; }
    public List<CartItem> Products { get; set; }

    public Cart(Guid id, DateTimeOffset createdAt, DateTimeOffset updatedAt, Guid customerId, List<CartItem> products) : base(id, createdAt, updatedAt)
    {
        CustomerId = customerId;
        Products = products;
    }
    
    // public override string ToString()
    // {
    //     return string.Format("cartId: {0}, cartCreatedAt: {4}, cartUpdatedAt: {5}, customerId: {1}, ", 
    //         Id, Name, NamePlural, UnitPrice, CreatedAt, UpdatedAt);
    // }
}