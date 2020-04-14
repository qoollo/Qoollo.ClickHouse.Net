using System;
using WebApiExample.Model;

namespace WebApiExample.Dto
{
    public class EntityDto
    {
        public EntityDto()
        {
        }

        public EntityDto(TestEntity testEntity)
        {
            Id = testEntity.Id;
            UserId = testEntity.UserId;
            TimeStamp = testEntity.TimeStamp;
            TestArray = testEntity.TestArray;
            Payload = testEntity.Payload;
        }

        public long Id { get; set; }
        public int UserId { get; set; }
        public DateTime TimeStamp { get; set; }
        public int[] TestArray { get; set; }
        public int? Payload { get; private set; }
    }
}
