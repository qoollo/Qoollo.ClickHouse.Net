using System.Collections.Generic;

namespace ConsoleExample.Readers
{
    public interface IReader
    {
        IEnumerable<Entity> SelectEntitiesByUserId(long userId);
    }
}