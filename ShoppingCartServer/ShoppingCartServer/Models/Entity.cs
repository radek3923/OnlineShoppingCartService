using ShoppingCartServer.Interfaces;

namespace ShoppingCartServer.Models;

public abstract class Entity  : IEntity
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}