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
    
    public override string ToString()
    {
        string cartItemList = String.Concat( Products.Select(o=>o.ToString() + "\n") );
        
        return string.Format("cartId: {0}, cartCreatedAt: {1}, cartUpdatedAt: {2}, customerId: {3}, List<cartItem>:\n{4} ", 
            Id, CreatedAt, UpdatedAt, CustomerId, cartItemList);
    }
}