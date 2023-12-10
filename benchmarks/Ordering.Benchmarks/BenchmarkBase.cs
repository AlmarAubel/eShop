using System.Data.Common;
using BenchmarkDotNet.Attributes;
using eShop.Ordering.API.Application.Queries;
using eShop.Ordering.API.Application.Queries.eShop.Ordering.API.Application.Queries;
using eShop.Ordering.Infrastructure;
using Npgsql;
using Ordering.Benchmarks.Database;

namespace Ordering.Benchmarks;

public class BenchmarkBase
{
    private DbConnection _connection = default!;
    protected OrderRawSqlQueries OrderRawSqlQueries = default!;
    protected OrderQueries OrderQueries = default!;
    protected OrderingContext OrderingContext = default!;
    
    protected SequentialIntGenerator NumberGenerator = default!;
    private OrderingContext _createDbContextWithLogger = default!;
    private List<string> _buyerIds= [];
    private int _count;

    [GlobalSetup]
    public async Task GlobalSetup()
    {
        var dbConnectionFactory = new DbConnectionFactory();
        _connection = await dbConnectionFactory.CreateConnectionAsync();
        
        OrderingContext = OrderingContext = DbContextFactory.CreateDbContext(_connection);
        OrderRawSqlQueries = new OrderRawSqlQueries(NpgsqlDataSource.Create(dbConnectionFactory.ConnectionString)); 
        
        await OrderingContext.Database.EnsureCreatedAsync();
        var seeder = new Seeder(OrderingContext);
        _buyerIds = await seeder.Seed();
        OrderingContext.ChangeTracker.Clear();
        NumberGenerator = new SequentialIntGenerator(1);

        _createDbContextWithLogger = DbContextFactory.CreateDbContextWithLogger(_connection);
        OrderQueries = new OrderQueries(_createDbContextWithLogger);
        
    }

    protected string GetbuyerId()
    {
        if( _count++ >= _buyerIds.Count) _count = 1;
        return _buyerIds[_count-1];
    }
    
    public void Back()=> _count--;
    
    [GlobalCleanup]
    public void GlobalCleanup()
    {
        _connection.Dispose();
    }
}
