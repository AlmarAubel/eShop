using System.Data.Common;
using eShop.Ordering.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ordering.Benchmarks.Database;

public static class DbContextFactory
{
    public static  OrderingContext CreateDbContextWithLogger(DbConnection connection)
    {
        var optionsBuilder = DefaultOptions(connection);

        var loggerFactory = GetLoggerFactory();
# if DEBUG
        optionsBuilder.UseLoggerFactory(loggerFactory);
#endif
        return new OrderingContext(optionsBuilder.Options);
    }

    public static  OrderingContext CreateDbContext(DbConnection connection)
    {
        var optionsBuilder = DefaultOptions(connection);
        return new OrderingContext(optionsBuilder.Options);
    }
    
    private static ILoggerFactory GetLoggerFactory()
    {
        var loggerFactory = LoggerFactory.Create(builder => 
        {
            builder
                .AddFilter((category, level) =>
                    category == DbLoggerCategory.Database.Command.Name
                    && level == LogLevel.Information)
                .AddConsole();
        });
        return loggerFactory;
    }

    private static DbContextOptionsBuilder<OrderingContext> DefaultOptions(DbConnection connection)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OrderingContext>();
        optionsBuilder.UseNpgsql(connection);
        return optionsBuilder;
    }
}
