using Microsoft.Extensions.Logging;
using Qoollo.ClickHouse.Net.Repository;
using System;
using System.Collections.Generic;

namespace Qoollo.ClickHouse.Net.AggregatingQueueProcessor
{
    public class ProcHolder<T> : IProcHolder<T>
    {
        public ProcHolder(Action<IClickHouseRepository, List<T>, ILogger> proc)
        {
            Proc = proc;
        }

        public Action<IClickHouseRepository, List<T>, ILogger> Proc { get; }
    }
}
