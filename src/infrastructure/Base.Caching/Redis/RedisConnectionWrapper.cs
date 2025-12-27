using System.Net;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Base.Caching.Redis;

public class RedisConnectionWrapper(

    IOptions<RedisCacheOptions> optionsAccessor,
    IConnectionMultiplexer connection
    
    ) : IRedisConnectionWrapper
{
    // FIELDS
    protected readonly SemaphoreSlim ConnectionLock = new(1, 1);
    protected volatile IConnectionMultiplexer Connection = connection;
    protected readonly RedisCacheOptions Options = optionsAccessor.Value;

    #region UTILITIES
    /// <summary>
    /// CREATE A NEW CONNECTION MULTIPLEXER INSTANCE
    /// </summary>
    protected virtual async Task<IConnectionMultiplexer> ConnectAsync()
    {
        IConnectionMultiplexer connection;

        if (Options.ConnectionMultiplexerFactory is null)
        {
            if (Options.ConfigurationOptions is not null)
                connection = await ConnectionMultiplexer.ConnectAsync(Options.ConfigurationOptions);
            else
                connection = await ConnectionMultiplexer.ConnectAsync(Options.Configuration);
        }
        else
        {
            connection = await Options.ConnectionMultiplexerFactory();
        }

        if (Options.ProfilingSession != null)
            connection.RegisterProfiler(Options.ProfilingSession);

        return connection;
    }
    /// <summary>
    /// CREATE A NEW CONNECTION MULTIPLEXER INSTANCE
    /// </summary>
    protected virtual IConnectionMultiplexer Connect()
    {
        IConnectionMultiplexer connection;

        if (Options.ConnectionMultiplexerFactory is null)
            connection = Options.ConfigurationOptions is not null ? ConnectionMultiplexer.Connect(Options.ConfigurationOptions) : ConnectionMultiplexer.Connect(Options.Configuration);
        else
            connection = Options.ConnectionMultiplexerFactory().GetAwaiter().GetResult();

        if (Options.ProfilingSession != null)
            connection.RegisterProfiler(Options.ProfilingSession);

        return connection;
    }
    /// <summary>
    /// GET CONNECTION TO REDIS SERVERS, AND RECONNECTS IF NECESSARY
    /// </summary>
    protected virtual async Task<IConnectionMultiplexer> GetConnectionAsync()
    {
        if (Connection?.IsConnected == true)
            return Connection;

        await ConnectionLock.WaitAsync();
        try
        {
            if (Connection?.IsConnected == true)
                return Connection;

            //Connection disconnected. Disposing connection...
            Connection?.Dispose();

            //Creating new instance of Redis Connection
            Connection = await ConnectAsync();
        }
        finally
        {
            ConnectionLock.Release();
        }

        return Connection;
    }
    /// <summary>
    /// GET CONNECTION TO REDIS SERVERS, AND RECONNECTS IF NECESSARY
    /// </summary>
    protected virtual IConnectionMultiplexer GetConnection()
    {
        if (Connection?.IsConnected == true)
            return Connection;

        ConnectionLock.Wait();
        try
        {
            if (Connection?.IsConnected == true)
                return Connection;

            //Connection disconnected. Disposing connection...
            Connection?.Dispose();

            //Creating new instance of Redis Connection
            Connection = Connect();
        }
        finally
        {
            ConnectionLock.Release();
        }

        return Connection;
    }
    #endregion

    // IMPLEMENTATION OF IRedisConnectionWrapper
    public string Instance => Options.InstanceName ?? string.Empty;

    public virtual async Task FlushDatabaseAsync()
    {
        var endPoints = await GetEndPointsAsync();

        await Task.WhenAll(endPoints.Select(async endPoint =>
        {
            var server = await GetServerAsync(endPoint);
            if (!server.IsReplica)
            {
                await server.FlushDatabaseAsync();
            }
        }));
    }

    public virtual IDatabase GetDatabase()
    {
        return GetConnection().GetDatabase();
    }

    public virtual async Task<IDatabase> GetDatabaseAsync()
    {
        return (await GetConnectionAsync()).GetDatabase();
    }

    public virtual async Task<EndPoint[]> GetEndPointsAsync()
    {
        return (await GetConnectionAsync()).GetEndPoints();
    }

    public virtual async Task<IServer> GetServerAsync(EndPoint endPoint)
    {
        return (await GetConnectionAsync()).GetServer(endPoint);
    }

    public virtual ISubscriber GetSubscriber()
    {
        return GetConnection().GetSubscriber();
    }

    public virtual async Task<ISubscriber> GetSubscriberAsync()
    {
        return (await GetConnectionAsync()).GetSubscriber();
    }

    public virtual void Dispose()
    {
        //dispose ConnectionMultiplexer
        Connection?.Dispose();
    }
}
