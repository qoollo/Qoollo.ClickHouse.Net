using System;

namespace WebApiExample.Dto
{
    public class PostEntityDto
    {
        public int UserId { get; set; }
        public DateTime TimeStamp { get; set; }
        public int[] TestArray { get; set; }
        public int? Payload { get; private set; }
    }
}
