using ClickHouse.Ado;
using Microsoft.Extensions.Logging;
using Qoollo.ClickHouse.Net.ConnectionPool.Configuration;
using System;
using System.Threading;

namespace Qoollo.ClickHouse.Net.ConnectionPool
{
    /// <summary>
    /// A connection pool that can work with a list of ClickHouse connection strings. 
    /// In case of a connection error, go to the next one from the list. 
    /// If all connection strings are invalid, an exception is thrown.
    /// </summary>
    /// <exception cref="ClickHouseConnectionException"></exception>
    public class ClickHouseConnectionPool : Turbo.ObjectPools.DynamicPoolManager<ClickHouseConnection>
    {
        private readonly ILogger _logger;
        private readonly string[] _connectionStrings;
        private volatile string _currentConnectionString;
        private readonly object _locker;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config">Configuration</param>
        /// <param name="logger">logger instance</param>
        public ClickHouseConnectionPool(IClickHouseConfiguration config, ILogger logger) : base(config.ConnectionPoolMaxCount, config.ConnectionPoolName)
        {
            if (config.ConnectionStrings == null)
                throw new ArgumentNullException(nameof(config.ConnectionStrings));

            if (config.ConnectionStrings.Count == 0)
                throw new ArgumentException("connectionStrings is empty", nameof(config.ConnectionStrings));

            _connectionStrings = config.ConnectionStrings.ToArray();
            _currentConnectionString = _connectionStrings[0];
            _logger = logger;
            _locker = new object();
        }

        protected override bool CreateElement(out ClickHouseConnection elem, int timeout, CancellationToken token)
        {
            var connectionString = _currentConnectionString;

            // try to do fast connect using _currentConnectionString
            while (connectionString != null)
            {
                if (TryOpenSingleConnection(connectionString, false, out ClickHouseConnection connection))
                {
                    _logger.LogInformation("Connection pool: {0} - successfully connected to DB instance, connection string: {1}", Name, connectionString);
                    elem = connection;
                    return true;
                }

                _logger.LogInformation("Connection pool: {0}: Db instance is Unavailable, connection string: {1}", Name, connectionString);

                Interlocked.CompareExchange(ref _currentConnectionString, null, connectionString);
                connectionString = _currentConnectionString;
            }

            // fast connect failed - start full connect using all connection strings
            elem = OpenNewConnection();
            return elem != null;
        }

        protected override void DestroyElement(ClickHouseConnection elem)
        {
            if (elem == null)
                return;

            try
            {
                if (elem.State == System.Data.ConnectionState.Open)
                    elem.Close();

                elem.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during connection close with ClickHouse in ClickHouseConnectionPool. Connection string: {0}", elem.ConnectionString);
            }

        }

        protected override bool IsValidElement(ClickHouseConnection elem)
        {
            if (_currentConnectionString == null)
                return false;

            return elem != null && elem.State == System.Data.ConnectionState.Open;
        }

        private bool TryOpenSingleConnection(string connectionString, bool throwErrorsFlag, out ClickHouseConnection connection)
        {
            var con = new ClickHouseConnection(connectionString);
            try
            {
                con.Open();
                connection = con;
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Connection pool: {0} can't establish connection. Connection string: {1}", Name, connectionString);
                con.Dispose();

                if (throwErrorsFlag)
                    throw new ClickHouseConnectionException("Error on connection to ClickHouse instance", ex);

                connection = null;
                return false;
            }
        }

        /// <summary>
        /// Pass through all connection strings in search of working. If not found, the last one will throw an exception
        /// </summary>
        /// <returns>New opened connection</returns>
        /// <exception cref="ClickHouseConnectionException"></exception>
        private ClickHouseConnection OpenNewConnection()
        {
            lock (_locker)
            {
                var connectionString = _currentConnectionString;

                // Check for multiple threads on lock - first will find new connection, others - connect to it without full scan. 
                if (connectionString != null && TryOpenSingleConnection(connectionString, false, out ClickHouseConnection connection))
                {
                    _logger.LogInformation("Connection pool: {0} - successfully connected to DB instance, connection string: {1}", Name, connectionString);
                    return connection;
                }

                //Current connectionString is nod valid.
                _currentConnectionString = null;

                for (int i = 0; i < _connectionStrings.Length; i++)
                {
                    _logger.LogInformation("Connection pool: {0}: try to connect to instance with connection string: {1}", Name, _connectionStrings[i]);

                    // Throw exception only if last connectionString also is not valid. 
                    bool throwError = i == _connectionStrings.Length - 1;

                    if (TryOpenSingleConnection(_connectionStrings[i], throwError, out connection))
                    {
                        _currentConnectionString = _connectionStrings[i];
                        _logger.LogInformation("Connection pool: {0}: new base connection string is: {1}", Name, _currentConnectionString);
                        return connection;
                    }
                }
                throw new Exception("Unexpected situation in ClickHouseConnectionPool");
            }
        }
    }
}
