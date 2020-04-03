using ClickHouse.Ado;
using Qoollo.ClickHouse.Net.ConnectionPool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Qoollo.ClickHouse.Net.Repository
{
    public class ClickHouseRepository : IClickHouseRepository
    {
        private readonly ClickHouseConnectionPool _clickHouseConnectionPool;

        public ClickHouseRepository(ClickHouseConnectionPool clickHouseConnectionPool)
        {
            _clickHouseConnectionPool = clickHouseConnectionPool;
        }

        public void BulkInsert<T>(string tableName, IEnumerable<string> columns, IEnumerable<T> bulk)
        {
            var queryText = $@"INSERT INTO {tableName}({string.Join(",", columns)}) VALUES @bulk";
            BulkInsert(queryText, bulk);
        }

        public Task BulkInsertAsync<T>(string tableName, IEnumerable<string> columns, IEnumerable<T> bulk)
        {
            return Task.Run(() => BulkInsert(tableName, columns, bulk));
        }

        public void BulkInsert(string tableName, IEnumerable<string> columns, IBulkInsertEnumerable bulk)
        {
            var queryText = $@"INSERT INTO {tableName}({string.Join(",", columns)}) VALUES @bulk";
            BulkInsert(queryText, bulk);
        }

        public Task BulkInsertAsync(string tableName, IEnumerable<string> columns, IBulkInsertEnumerable bulk)
        {
            return Task.Run(() => BulkInsert(tableName, columns, bulk));
        }

        public void BulkInsert<T>(string queryText, IEnumerable<T> bulk)
        {
            using (var item = _clickHouseConnectionPool.Rent())
            {
                using (var cmd = item.Element.CreateCommand(queryText))
                {
                    cmd.Parameters.Add(new ClickHouseParameter
                    {
                        ParameterName = "bulk",
                        Value = bulk
                    });
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public Task BulkInsertAsync<T>(string queryText, IEnumerable<T> bulk)
        {
            return Task.Run(() => BulkInsert(queryText, bulk));
        }

        public void BulkInsert(string queryText, IBulkInsertEnumerable bulk)
        {
            //TODO check
            using (var item = _clickHouseConnectionPool.Rent())
            {
                using (var cmd = item.Element.CreateCommand(queryText))
                {
                    cmd.Parameters.Add(new ClickHouseParameter
                    {
                        ParameterName = "bulk",
                        Value = bulk
                    });
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public Task BulkInsertAsync(string queryText, IBulkInsertEnumerable bulk)
        {
            return Task.Run(() => BulkInsert(queryText, bulk));
        }

        public void ExecuteNonQuery(string queryText, IEnumerable<ClickHouseParameter> parameters = null)
        {
            using (var item = _clickHouseConnectionPool.Rent())
            {
                using (var cmd = item.Element.CreateCommand(queryText))
                {
                    AddParametersToCommand(cmd, parameters);

                    cmd.ExecuteNonQuery();
                }
            }
        }
        
        public Task ExecuteNonQueryAsync(string queryText, IEnumerable<ClickHouseParameter> parameters = null)
        {
            return Task.Run(() => ExecuteNonQuery(queryText, parameters));
        }

        public IEnumerable<T> ExecuteQueryMapping<T>(string queryText, IEntityMapper<T> mapper, IEnumerable<ClickHouseParameter> parameters = null)
        {
            var result = new List<T>();
            using (var item = _clickHouseConnectionPool.Rent())
            {
                using (var cmd = item.Element.CreateCommand(queryText))
                {
                    AddParametersToCommand(cmd, parameters);

                    using (var reader = cmd.ExecuteReader())
                    {
                        do
                        {
                            while (reader.Read())
                                result.Add(mapper.MapEntity(reader));
                        } while (reader.NextResult());
                    }
                }
            }
            return result;
        }

        public Task<IEnumerable<T>> ExecuteQueryMappingAsync<T>(string queryText, IEntityMapper<T> mapper, IEnumerable<ClickHouseParameter> parameters = null)
        {
            return Task.Run(() => ExecuteQueryMapping(queryText, mapper, parameters));
        }

        public T ExecuteReader<T>(string queryText, Func<IDataReader, T> processor, IEnumerable<ClickHouseParameter> parameters = null)
        {
            using (var item = _clickHouseConnectionPool.Rent())
            {
                using (var cmd = item.Element.CreateCommand(queryText))
                {
                    AddParametersToCommand(cmd, parameters);

                    using (var reader = cmd.ExecuteReader())
                    {
                        return processor.Invoke(reader);
                    }
                }
            }
        }

        public Task<T> ExecuteReaderAsync<T>(string queryText, Func<IDataReader, T> processor, IEnumerable<ClickHouseParameter> parameters = null)
        {
            return Task.Run(() => ExecuteReaderAsync(queryText, processor, parameters));
        }

        public object ExecuteScalar(string queryText, IEnumerable<ClickHouseParameter> parameters = null)
        {
            using (var item = _clickHouseConnectionPool.Rent())
            {
                using (var cmd = item.Element.CreateCommand(queryText))
                {
                    AddParametersToCommand(cmd, parameters);
                    return cmd.ExecuteScalar();
                }
            }
        }

        public Task<object> ExecuteScalarAsync(string queryText, IEnumerable<ClickHouseParameter> parameters = null)
        {
            return Task.Run(() => ExecuteScalar(queryText, parameters));
        }

        private void AddParametersToCommand(ClickHouseCommand cmd, IEnumerable<ClickHouseParameter> parameters)
        {
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
            }
        }
    }
}