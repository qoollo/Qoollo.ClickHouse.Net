using Microsoft.Extensions.Logging;
using Qoollo.ClickHouse.Net.Repository;
using System;
using System.Collections.Generic;

namespace Qoollo.ClickHouse.Net.AggregatingQueueProcessor
{
    public interface IProcHolder<T>
    {
        Action<IClickHouseRepository, List<T>, ILogger> Proc { get; }
    }
}