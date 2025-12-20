using StackExchange.Redis;
using System.Net;

namespace Base.Infrastructure.Caching;

public interface IRedisConnectionWrapper
{
    /// <summary>
    /// THE REDIS INSTANCE NAME
    /// </summary>
    string Instance { get; }
    /// <summary>
    /// OBTAIN AN INTERACTIVE CONNECTION TO A DATABASE INSIDE REDIS
    /// </summary>
    Task<IDatabase> GetDatabaseAsync();
    /// <summary>
    /// OBTAIN AN INTERACTIVE CONNECTION TO A DATABASE INSIDE REDIS
    /// </summary>
    IDatabase GetDatabase();
    /// <summary>
    /// OBTAIN A CONFIGURATION API FOR AN INDIVIDUAL SERVER
    /// </summary>
    Task<IServer> GetServerAsync(EndPoint endPoint);
    /// <summary>
    /// GETS ALL ENDPOINTS DEFINED ON THE SERVER
    /// </summary>
    Task<EndPoint[]> GetEndPointsAsync();
    /// <summary>
    /// GETS A SUBSCRIBER FOR THE SERVER
    /// </summary>
    Task<ISubscriber> GetSubscriberAsync();
    /// <summary>
    /// GETS A SUBSCRIBER FOR THE SERVER
    /// </summary>
    ISubscriber GetSubscriber();
    /// <summary>
    /// DELETE ALL THE KEYS OF THE DATABASE
    /// </summary>
    Task FlushDatabaseAsync();
}
