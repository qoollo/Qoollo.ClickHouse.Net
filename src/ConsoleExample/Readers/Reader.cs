using ClickHouse.Ado;
using Qoollo.ClickHouse.Net.Repository;
using System.Collections.Generic;

namespace ConsoleExample.Readers
{
    public class Reader : IReader
    {
        private readonly IClickHouseRepository _clickHouseRepository;

        private readonly string _selectByUserIdQuery = $"SELECT * FROM {Entity.TableName} WHERE userId = @userId";

        public Reader(IClickHouseRepository clickHouseRepository)
        {
            _clickHouseRepository = clickHouseRepository;
        }

        public IEnumerable<Entity> SelectEntitiesByUserId(long userId)
        {
            var mapper = new EntityMapper();

            var parameters = new List<ClickHouseParameter>()
            {
                new ClickHouseParameter()
                {
                    ParameterName = "userId",
                    Value = userId,
                    DbType = System.Data.DbType.Int64
                }
            };

            return _clickHouseRepository.ExecuteQueryMapping(_selectByUserIdQuery, mapper, parameters);
        }
    }
}
