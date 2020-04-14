# Qoollo.ClickHouse.Net

Qoollo.ClickHouse.Net is a library of useful classes for working with ClickHouse in .NET Core.

## Key features of Qoollo.ClickHouse.Net
- The thread pool (`ClickHouseConnectionPool`), working with a list of connection strings and automatically reconnecting in the event of a disconnection.
- Repository (`IClickHouseRepository`), using a thread pool and allowing:
    - simply execute the most common queries (BULK INSERT, SELECT with mapping to the entity).
    - execute an arbitrary request using the capabilities of the [ClickHouse-Net](https://github.com/killwort/ClickHouse-Net) driver.
- Aggregating queue with a set of worker threads (`IClickHouseAggregatingQueueProcessor`). The most common scenario is the aggregation of single incoming events into packages for writing to ClickHouse.
- ServiceCollectionExtensions for easy connectivity in .NET Core.

In this library, [ClickHouse-Net](https://github.com/killwort/ClickHouse-Net) is used as a driver, that implements the native protocol, which positively affects performance.

## Wiki
See the [wiki](https://github.com/qoollo/Qoollo.ClickHouse.Net/wiki) for more information about using the library and code samples.  

***

## Qoollo.ClickHouse.Net по русски

Qoollo.ClickHouse.Net - библиотека полезных классов для работы с ClickHouse в .NET Core.

## Основные особенности Qoollo.ClickHouse.Net
- Пул потоков (`ClickHouseConnectionPool`), работающий со списком строк подключений и автоматически переподключающийся в случае разрыва соединения. 
- Репозиторий (`IClickHouseRepository`), использующий пул потоков и позволяющий:
    - просто выполнять наиболее типовые запросы (BULK INSERT, SELECT с мапингом на сущность).
    - выполнить произвольный запрос, используя возможности драйвера [ClickHouse-Net](https://github.com/killwort/ClickHouse-Net). 
- Агрегирующая очередь с набором потоков worker-ов (`IClickHouseAggregatingQueueProcessor`). Наиболее частый сценарий - агрегация одиночных приходящих событий в пакеты для записи в ClickHouse. 
- ServiceCollectionExtensions для простого подключения в .NET Core. 

В качестве драйвера используется [ClickHouse-Net](https://github.com/killwort/ClickHouse-Net) реализующий нативный протокол, что положительно сказывается на производительности.

## Wiki
Более подробное описание библиотеки и примеры кода вы можете найти в [wiki](https://github.com/qoollo/Qoollo.ClickHouse.Net/wiki).

