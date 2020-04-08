using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleExample.Writers
{
    public interface IWriter
    {
        void CreateEntityTableIfNotExists();
        Task CreateEntityTableIfNotExistsAsync();
        void WriteEntities(IEnumerable<Entity> entities);
        Task WriteEntitiesAsync(IEnumerable<Entity> entities);
    }
}