using ClickHouse.Ado;
using System;
using System.Collections.Generic;
using System.Data;

namespace Qoollo.ClickHouse.Net.Repository
{
    /// <summary>
    /// A repository that implements convenient wrappers for common use cases of ClickHouse. 
    /// Also provides the ability to execute an arbitrary query.
    /// </summary>
    public interface IClickHouseRepository
    {
        /// <summary>
        /// Batch Insert Method, allows customizing query. Warning - the wrong query can be a reason for the exceptions.
        /// </summary>
        /// <typeparam name="T">Entity type, should implement IEnumerable</typeparam>
        /// <param name="queryText">Text of insert query. The query should contain @bulk parameter</param>
        /// <param name="bulk">Collection for insert</param>
        void BulkInsert<T>(string queryText, IEnumerable<T> bulk);

        /// <summary>
        /// Batch Insert Method. Generates query from table name and row names. 
        /// </summary>
        /// <typeparam name="T">Entity type, should implement IEnumerable</typeparam>
        /// <param name="tableName">The name of the table into which data will be inserted.</param>
        /// <param name="columns">A list of column names into which data will be inserted. Order is important!</param>
        /// <param name="bulk">Collection for insert.</param>
        void BulkInsert<T>(string tableName, IEnumerable<string> columns, IEnumerable<T> bulk);

        /// <summary>
        /// Batch Insert Method, allows customizing query. Warning - the wrong query can be a reason for the exceptions.
        /// Uses IBulkInsertEnumerable to speed up processing and use less memory inside ClickHouse driver. 
        /// </summary>
        /// <param name="queryText">Text of insert query. The query should contain @bulk parameter</param>
        /// <param name="bulk">Collection for insert.</param>
        void BulkInsert(string queryText, IBulkInsertEnumerable bulk);

        /// <summary>
        /// Batch Insert Method. Generates query from table name and row names. 
        /// Uses IBulkInsertEnumerable to speed up processing and use less memory inside ClickHouse driver. 
        /// </summary>
        /// <param name="tableName">The name of the table into which data will be inserted.</param>
        /// <param name="columns">A list of column names into which data will be inserted. Order is important!</param>
        /// <param name="bulk">Collection for insert.</param>
        void BulkInsert(string tableName, IEnumerable<string> columns, IBulkInsertEnumerable bulk);

        /// <summary>
        /// Method for executing query without result.
        /// </summary>
        /// <param name="queryText">Text for select query. The query should contain all parameters from parameters list.</param>
        /// <param name="parameters">Collection of parameters for query.</param>
        void ExecuteNonQuery(string queryText, IEnumerable<ClickHouseParameter> parameters = null);

        /// <summary>
        /// Method for executing query and read first result.
        /// </summary>
        /// <param name="queryText">Text for select query. The query should contain all parameters from parameters list.</param>
        /// <param name="parameters">Collection of parameters for query.</param>
        /// <returns>first result without any mapping.</returns>
        object ExecuteScalar(string queryText, IEnumerable<ClickHouseParameter> parameters = null);

        /// <summary>
        /// Method for executing a custom query processor.
        /// </summary>
        /// <typeparam name="T">Result type for processor.</typeparam>
        /// <param name="queryText">Text for select query. The query should contain all parameters from parameters list.</param>
        /// <param name="processor">Custom query processor.</param>
        /// <param name="parameters">Collection of parameters for query.</param>
        /// <returns>Result of processor.</returns>
        T ExecuteReader<T>(string queryText, Func<IDataReader, T> processor, IEnumerable<ClickHouseParameter> parameters = null);

        /// <summary>
        /// Method for executing a SELECT query that maps the received data to a specific type of entity.
        /// </summary>
        /// <typeparam name="T">The type of entity on which the data will be mapped.</typeparam>
        /// <param name="queryText">Text for select query. The query should contain all parameters from parameters list.</param>
        /// <param name="mapper">Mapper implementation for type T.</param>
        /// <param name="parameters">Collection of parameters for query.</param>
        /// <returns>Collection of entities.</returns>
        IEnumerable<T> ExecuteQueryMapping<T>(string queryText, IEntityMapper<T> mapper, IEnumerable<ClickHouseParameter> parameters = null);
    }
}