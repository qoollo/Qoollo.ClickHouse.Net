using ClickHouse.Ado;
using Qoollo.ClickHouse.Net.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleExample.Writers
{
    public class Writer : IWriter
    {
        private readonly string _tableName = Entity.TableName;
        private readonly List<string> _columnNames = Entity.ColumnNames;
        private readonly string _createTableQuery = Entity.CreateTableQuery;

        private readonly IClickHouseRepository _clickHouseRepository;

        public Writer(IClickHouseRepository clickHouseRepository)
        {
            _clickHouseRepository = clickHouseRepository;
        }

        public void WriteEntities(IEnumerable<Entity> entities)
        {
            _clickHouseRepository.BulkInsert(_tableName, _columnNames, entities);
        }

        public Task WriteEntitiesAsync(IEnumerable<Entity> entities)
        {
            return _clickHouseRepository.BulkInsertAsync(_tableName, _columnNames, entities);
        }

        public void CreateTableIfNotExists()
        {
            _clickHouseRepository.ExecuteNonQuery(_createTableQuery);
        }

        public Task CreateTableIfNotExistsAsync()
        {
            return _clickHouseRepository.ExecuteNonQueryAsync(_createTableQuery);
        }
    }
}
