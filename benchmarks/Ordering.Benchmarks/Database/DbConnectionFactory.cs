using System.Data;
using System.Data.Common;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Ordering.Benchmarks.Database;

public class DbConnectionFactory
{
    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithDatabase("ordering")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task<DbConnection> CreateConnectionAsync()
    {
        if (string.IsNullOrEmpty(ConnectionString))
        {
            await _container.StartAsync();
            ConnectionString = _container.GetConnectionString();
        }

        var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
            
        return connection;
    }
    
    
}


