using Base.Infrastructure.Caching;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Net;

namespace Base.Infrastructure.Caching;

public class RedisConnectionWrapper(

    IOptions<RedisCacheOptions> optionsAccessor

    ) : IRedisConnectionWrapper
{
    // FIELDS
    protected readonly SemaphoreSlim _connectionLock = new(1, 1);
    protected volatile IConnectionMultiplexer _connection;
    protected readonly RedisCacheOptions _options = optionsAccessor.Value;

    #region UTILITIES
    /// <summary>
    /// CREATE A NEW CONNECTION MULTIPLEXER INSTANCE
    /// </summary>
    protected virtual async Task<IConnectionMultiplexer> ConnectAsync()
    {
        IConnectionMultiplexer connection;

        if (_options.ConnectionMultiplexerFactory is null)
        {
            if (_options.ConfigurationOptions is not null)
                connection = await ConnectionMultiplexer.ConnectAsync(_options.ConfigurationOptions);
            else
                connection = await ConnectionMultiplexer.ConnectAsync(_options.Configuration);
        }
        else
        {
            connection = await _options.ConnectionMultiplexerFactory();
        }

        if (_options.ProfilingSession != null)
            connection.RegisterProfiler(_options.ProfilingSession);

        return connection;
    }
    /// <summary>
    /// CREATE A NEW CONNECTION MULTIPLEXER INSTANCE
    /// </summary>
    protected virtual IConnectionMultiplexer Connect()
    {
        IConnectionMultiplexer connection;

        if (_options.ConnectionMultiplexerFactory is null)
            connection = _options.ConfigurationOptions is not null ? ConnectionMultiplexer.Connect(_options.ConfigurationOptions) : ConnectionMultiplexer.Connect(_options.Configuration);
        else
            connection = _options.ConnectionMultiplexerFactory().GetAwaiter().GetResult();

        if (_options.ProfilingSession != null)
            connection.RegisterProfiler(_options.ProfilingSession);

        return connection;
    }
    /// <summary>
    /// GET CONNECTION TO REDIS SERVERS, AND RECONNECTS IF NECESSARY
    /// </summary>
    protected virtual async Task<IConnectionMultiplexer> GetConnectionAsync()
    {
        if (_connection?.IsConnected == true)
            return _connection;

        await _connectionLock.WaitAsync();
        try
        {
            if (_connection?.IsConnected == true)
                return _connection;

            //Connection disconnected. Disposing connection...
            _connection?.Dispose();

            //Creating new instance of Redis Connection
            _connection = await ConnectAsync();
        }
        finally
        {
            _connectionLock.Release();
        }

        return _connection;
    }
    /// <summary>
    /// GET CONNECTION TO REDIS SERVERS, AND RECONNECTS IF NECESSARY
    /// </summary>
    protected virtual IConnectionMultiplexer GetConnection()
    {
        if (_connection?.IsConnected == true)
            return _connection;

        _connectionLock.Wait();
        try
        {
            if (_connection?.IsConnected == true)
                return _connection;

            //Connection disconnected. Disposing connection...
            _connection?.Dispose();

            //Creating new instance of Redis Connection
            _connection = Connect();
        }
        finally
        {
            _connectionLock.Release();
        }

        return _connection;
    }
    #endregion

    // IMPLEMENTATION OF IREDISCONNECTIONWRAPPER INTERFACE
    public string Instance => _options.InstanceName ?? string.Empty;

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
        _connection?.Dispose();
    }
}
