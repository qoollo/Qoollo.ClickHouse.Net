using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace WebApiExample.Model
{
    public class TestEntity : IEnumerable
    {
        public long Id { get; set; }
        public int UserId { get; set; }
        public DateTime TimeStamp { get; set; }
        public int[] TestArray { get; set; }
        public int? Payload { get; private set; }

        public TestEntity(IDataReader reader)
        {
            Id = reader.GetInt64(reader.GetOrdinal("id"));
            UserId = reader.GetInt32(reader.GetOrdinal("userId"));
            TimeStamp = reader.GetDateTime(reader.GetOrdinal("timeStamp"));
            var objects = (object[]) reader.GetValue(reader.GetOrdinal("testArray"));
            TestArray = objects.Select(o => (int)o).ToArray();
            int index = reader.GetOrdinal("payload");
            Payload = reader.IsDBNull(index) ? (int?)null : reader.GetInt32(index);
        }

        public TestEntity(int userId, long id, DateTime timeStamp, int[] testArray, int? payload)
        {
            Id = id;
            UserId = userId;
            TimeStamp = timeStamp;
            TestArray = testArray;
            Payload = payload;
        }

        public TestEntity()
        { 
        }

        public IEnumerator GetEnumerator()
        {
            yield return Id;
            yield return UserId;
            yield return TimeStamp;
            yield return TestArray;
            yield return Payload;
        }

        public override string ToString()
        {
            return $"Id: {Id} | User: {UserId} |{TimeStamp}| arr: {TestArray}| {Payload}";
        }

        public static List<string> ColumnNames = new List<string>() { "id", "userId", "timeStamp", "testArray", "payload" };
        public static string TableName = "TestTableEntity";

        public static string CreateTableQuery = @$"CREATE TABLE IF NOT EXISTS {TableName} (
                        id Int64,
                        userId Int32,
                        timeStamp DateTime64(3),
                        testArray Array(Int32),
                        payload Nullable(Int32))
                        ENGINE=MergeTree()
                        PARTITION BY toYYYYMM(timeStamp)
                        ORDER BY (timeStamp, userId);";
    }
}
