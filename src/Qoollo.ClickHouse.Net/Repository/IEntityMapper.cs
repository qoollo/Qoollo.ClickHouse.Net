using System.Data;

namespace Qoollo.ClickHouse.Net.Repository
{
    /// <summary>
    /// Interface for mapper class. 
    /// </summary>
    /// <typeparam name="T">Type of entity</typeparam>
    public interface IEntityMapper<T>
    {
        /// <summary>
        /// Implements reading entity from reader logic.
        /// </summary>
        /// <param name="reader">reader</param>
        /// <returns>Entity</returns>
        T MapEntity(IDataReader reader);
    }
}