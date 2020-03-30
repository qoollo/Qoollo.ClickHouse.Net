using System.Data;

namespace Qoollo.ClickHouse.Net.Repository
{
    public interface IEntityMapper<T>
    {
        T MapEntity(IDataReader reader);
    }
}