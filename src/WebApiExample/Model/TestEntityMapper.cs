using Qoollo.ClickHouse.Net.Repository;
using System.Data;

namespace WebApiExample.Model
{
    public class TestEntityMapper : IEntityMapper<TestEntity>
    {
        public TestEntity MapEntity(IDataReader reader)
        {
            return new TestEntity(reader);
        }
    }
}
