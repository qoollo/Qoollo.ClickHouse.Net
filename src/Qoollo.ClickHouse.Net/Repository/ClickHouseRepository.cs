using ClickHouse.Ado;
using Qoollo.ClickHouse.Net.ConnectionPool;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Qoollo.ClickHouse.Net.Repository
{
    /// <summary>
    /// A repository that implements convenient wrappers for common use cases of ClickHouse. 
    /// Also provides the ability to execute an arbitrary query.
    /// </summary>
    public class ClickHouseRepository : IClickHouseRepository
    {
        private readonly ClickHouseConnectionPool _clickHouseConnectionPool;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clickHouseConnectionPool">connection pool</param>
        public ClickHouseRepository(ClickHouseConnectionPool clickHouseConnectionPool)
        {
            _clickHouseConnectionPool = clickHouseConnectionPool;
        }

        /// <summary>
        /// Bulk insert, generates query from table name and row names.
        /// </summary>
        /// <typeparam name="T">Entity type, should implement IEnumerable.</typeparam>
        /// <param name="tableName">The name of the table into which data will be inserted.</param>
        /// <param name="columns">A list of column names into which data will be inserted. Order is important!</param>
        /// <param name="bulk">Collection for insert.</param>
        public void BulkInsert<T>(string tableName, IEnumerable<string> columns, IEnumerable<T> bulk)
        {
            var queryText = $@"INSERT INTO {tableName}({string.Join(",", columns)}) VALUES @bulk";
            BulkInsert(queryText, bulk);
        }

        /// <summary>
        /// Asynchronous bulk insert, generates query from table name and row names.
        /// </summary>
        /// <typeparam name="T">Entity type, should implement IEnumerable.</typeparam>
        /// <param name="tableName">The name of the table into which data will be inserted</param>
        /// <param name="columns">A list of column names into which data will be inserted. Order is important!</param>
        /// <param name="bulk">Collection for insert</param>
        public Task BulkInsertAsync<T>(string tableName, IEnumerable<string> columns, IEnumerable<T> bulk)
        {
            return Task.Run(() => BulkInsert(tableName, columns, bulk));
        }

        /// <summary>
        /// Bulk insert, generates query from table name and row names. 
        /// Uses IBulkInsertEnumerable to speed up processing and use less memory inside ClickHouse driver. 
        /// </summary>
        /// <param name="tableName">The name of the table into which data will be inserted.</param>
        /// <param name="columns">A list of column names into which data will be inserted. Order is important!</param>
        /// <param name="bulk">Collection for insert.</param>
        public void BulkInsert(string tableName, IEnumerable<string> columns, IBulkInsertEnumerable bulk)
        {
            var queryText = $@"INSERT INTO {tableName}({string.Join(",", columns)}) VALUES @bulk";
            BulkInsert(queryText, bulk);
        }

        /// <summary>
        /// Asynchronous bulk insert, generates query from table name and row names. 
        /// Uses IBulkInsertEnumerable to speed up processing and use less memory inside ClickHouse driver. 
        /// </summary>
        /// <param name="tableName">The name of the table into which data will be inserted.</param>
        /// <param name="columns">A list of column names into which data will be inserted. Order is important!</param>
        /// <param name="bulk">Collection for insert.</param>
        public Task BulkInsertAsync(string tableName, IEnumerable<string> columns, IBulkInsertEnumerable bulk)
        {
            return Task.Run(() => BulkInsert(tableName, columns, bulk));
        }

        /// <summary>
        /// Bulk insert, allows customizing query. Warning - the wrong query can be a reason for the exceptions.
        /// </summary>
        /// <typeparam name="T">Entity type, should implement IEnumerable.</typeparam>
        /// <param name="queryText">Text of insert query. The query should contain @bulk parameter.</param>
        /// <param name="bulk">Collection for insert.</param>
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

        /// <summary>
        /// Asynchronous bulk insert, allows customizing query. Warning - the wrong query can be a reason for the exceptions.
        /// </summary>
        /// <typeparam name="T">Entity type, should implement IEnumerable.</typeparam>
        /// <param name="queryText">Text of insert query. The query should contain @bulk parameter.</param>
        /// <param name="bulk">Collection for insert.</param>
        public Task BulkInsertAsync<T>(string queryText, IEnumerable<T> bulk)
        {
            return Task.Run(() => BulkInsert(queryText, bulk));
        }

        /// <summary>
        /// Asynchronous bulk insert, allows customizing query. Warning - the wrong query can be a reason for the exceptions.
        /// Uses IBulkInsertEnumerable to speed up processing and use less memory inside ClickHouse driver. 
        /// </summary>
        /// <param name="queryText">Text of insert query. The query should contain @bulk parameter.</param>
        /// <param name="bulk">Collection for insert.</param>
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

        /// <summary>
        /// Bulk insert, allows customizing query. Warning - the wrong query can be a reason for the exceptions.
        /// Uses IBulkInsertEnumerable to speed up processing and use less memory inside ClickHouse driver. 
        /// </summary>
        /// <param name="queryText">Text of insert query. The query should contain @bulk parameter</param>
        /// <param name="bulk">Collection for insert.</param>
        public Task BulkInsertAsync(string queryText, IBulkInsertEnumerable bulk)
        {
            return Task.Run(() => BulkInsert(queryText, bulk));
        }

        /// <summary>
        /// Method for executing query without result.
        /// </summary>
        /// <param name="queryText">Text for select query. The query should contain all parameters from parameters list.</param>
        /// <param name="parameters">Collection of parameters for query.</param>
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

        /// <summary>
        /// Asynchronous method for executing query without result.
        /// </summary>
        /// <param name="queryText">Text for select query. The query should contain all parameters from parameters list.</param>
        /// <param name="parameters">Collection of parameters for query.</param>
        public Task ExecuteNonQueryAsync(string queryText, IEnumerable<ClickHouseParameter> parameters = null)
        {
            return Task.Run(() => ExecuteNonQuery(queryText, parameters));
        }

        /// <summary>
        /// Method for executing a select query that maps the received data to a specific type of entity.
        /// </summary>
        /// <typeparam name="T">The type of entity on which the data will be mapped.</typeparam>
        /// <param name="queryText">Text for select query. The query should contain all parameters from parameters list.</param>
        /// <param name="mapper">Mapper implementation for type T.</param>
        /// <param name="parameters">Collection of parameters for query.</param>
        /// <returns>Collection of entities</returns>
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

        /// <summary>
        /// Asynchronous method for executing a select query that maps the received data to a specific type of entity.
        /// </summary>
        /// <typeparam name="T">The type of entity on which the data will be mapped.</typeparam>
        /// <param name="queryText">Text for select query. The query should contain all parameters from parameters list.</param>
        /// <param name="mapper">Mapper implementation for type T.</param>
        /// <param name="parameters">Collection of parameters for query.</param>
        /// <returns>Collection of entities</returns>
        public Task<IEnumerable<T>> ExecuteQueryMappingAsync<T>(string queryText, IEntityMapper<T> mapper, IEnumerable<ClickHouseParameter> parameters = null)
        {
            return Task.Run(() => ExecuteQueryMapping(queryText, mapper, parameters));
        }

        /// <summary>
        /// Method for executing a custom query processor
        /// </summary>
        /// <typeparam name="T">Result type for processor.</typeparam>
        /// <param name="queryText">Text for select query. The query should contain all parameters from parameters list.</param>
        /// <param name="processor">Custom query processor.</param>
        /// <param name="parameters">Collection of parameters for query.</param>
        /// <returns>Result of processor</returns>
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

        /// <summary>
        /// Asynchronous method for executing a custom query processor.
        /// </summary>
        /// <typeparam name="T">Result type for processor.</typeparam>
        /// <param name="queryText">Text for select query. The query should contain all parameters from parameters list.</param>
        /// <param name="processor">Custom query processor.</param>
        /// <param name="parameters">Collection of parameters for query.</param>
        /// <returns>Result of processor</returns>
        public Task<T> ExecuteReaderAsync<T>(string queryText, Func<IDataReader, T> processor, IEnumerable<ClickHouseParameter> parameters = null)
        {
            return Task.Run(() => ExecuteReaderAsync(queryText, processor, parameters));
        }

        /// <summary>
        /// Method for executing query and read first result.
        /// </summary>
        /// <param name="queryText">Text for select query. The query should contain all parameters from parameters list.</param>
        /// <param name="parameters">Collection of parameters for query.</param>
        /// <returns>first result without any mapping</returns>
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

        /// <summary>
        /// Asynchronous method for executing query and read first result.
        /// </summary>
        /// <param name="queryText">Text for select query. The query should contain all parameters from parameters list.</param>
        /// <param name="parameters">Collection of parameters for query.</param>
        /// <returns>first result without any mapping</returns>
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