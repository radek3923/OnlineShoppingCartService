namespace ShoppingCartServer.Interfaces;

public interface IDateTimeOffsetGenerator
{
    public DateTimeOffset GetActualDateTimeOffset();
}