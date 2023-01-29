using ShoppingCartServer.Interfaces;
using ShoppingCartServer.Utils;

namespace ShoppingCartServer.Models;

public abstract class Entity  : IEntity
{
    [Order(1)]
    public Guid Id { get; set; }
    [Order(2)]
    public DateTimeOffset CreatedAt { get; set; }
    [Order(3)]
    public DateTimeOffset UpdatedAt { get; set; }

    protected Entity(Guid id, DateTimeOffset createdAt, DateTimeOffset updatedAt)
    {
        Id = id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}