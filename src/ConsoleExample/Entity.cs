using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace ConsoleExample
{
    public class Entity : IEnumerable
    {
        public long UserId { get; set; }
        public DateTime TimeStamp { get; set; }
        public int ActionId { get; set; }
        public double? Latitude { get; private set; }
        public double? Longitude { get; private set; }

        public Entity(IDataReader reader)
        {
            UserId = reader.GetInt64(reader.GetOrdinal("userId"));
            TimeStamp = reader.GetDateTime(reader.GetOrdinal("timeStamp"));
            ActionId = reader.GetInt32(reader.GetOrdinal("actionId"));
            int index = reader.GetOrdinal("latitude");
            Latitude = reader.IsDBNull(index) ? (double?) null : reader.GetDouble(index);
            index = reader.GetOrdinal("longitude");
            Longitude = reader.IsDBNull(index) ? (double?) null : reader.GetDouble(index);
        }

        public Entity(long userId, DateTime timeStamp, int actionId, double? latitude, double? longitude)
        {
            UserId = userId;
            TimeStamp = timeStamp;
            ActionId = actionId;
            Latitude = latitude;
            Longitude = longitude;
        }

        public IEnumerator GetEnumerator()
        {
            yield return UserId;
            yield return TimeStamp;
            yield return ActionId;
            yield return Latitude;
            yield return Longitude;
        }

        public override string ToString()
        {
            return $"User: {UserId} |{TimeStamp}| Action: {ActionId}| {Latitude}, {Longitude}";
        }

        public static List<string> ColumnNames = new List<string>() { "userId", "timeStamp", "actionId", "latitude", "longitude" };
        public static string TableName = "TestEntity";

        public static string CreateTableQuery = @$"CREATE TABLE IF NOT EXISTS {TableName} (
                        userId Int64,
                        timeStamp DateTime,
                        actionId Int32,
                        latitude Nullable(Float64),
                        longitude Nullable(Float64))
                        ENGINE=MergeTree()
                        PARTITION BY toYYYYMM(timeStamp)
                        ORDER BY (timeStamp, userId);";
    }
}
