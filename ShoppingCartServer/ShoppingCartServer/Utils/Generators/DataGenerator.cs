using ShoppingCartServer.Interfaces;

namespace ShoppingCartServer.Utils.Generators;

public class DataGenerator : IGuIdGenerator, IDateTimeOffsetGenerator
{
    
    public DateTimeOffset GetActualDateTimeOffset()
    {
        return DateTimeOffset.Now;
    }
    
    public Guid getNewGuID()
    {
        return Guid.NewGuid();
    }
}