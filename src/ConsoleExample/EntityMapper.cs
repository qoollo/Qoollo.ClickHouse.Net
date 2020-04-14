using Qoollo.ClickHouse.Net.Repository;
using System.Data;

namespace ConsoleExample
{
    public class EntityMapper : IEntityMapper<Entity>
    {
        public Entity MapEntity(IDataReader reader)
        {
            return new Entity(reader);
        }
    }
}
