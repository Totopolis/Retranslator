using Domain.Entities.JsonRequest;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Persistence.Repositories;

public class RetranslatorDbContext : DbContext
{
    private readonly string _connectionString;
    private readonly ILoggerFactory _loggerFactory;
    private readonly PublishDomainEventsToEventBusInterceptor _interceptor;

    public RetranslatorDbContext(
        IOptions<PostgreSettings> settings,
        ILoggerFactory loggerFactory,
        PublishDomainEventsToEventBusInterceptor interceptor)
    {
        _connectionString = settings.Value.ConnectionString;
        _loggerFactory = loggerFactory;
        _interceptor = interceptor;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql(_connectionString)
            .AddInterceptors(_interceptor)
            .UseLoggerFactory(_loggerFactory)
            .EnableSensitiveDataLogging();

        // TODO: use utc - need avoid
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");

        var entity = modelBuilder.Entity<JsonRequest>();
        entity
            .ToTable("json_request")
            .HasKey(x => x.Id)
            .HasName("key_json_request_id");

        entity
            .Property(x => x.Id)
            .HasConversion(id => id.Id, val => new JsonRequestId(val));

        entity
            .Property(x => x.Content)
            .HasColumnName("content")
            .IsRequired();

        entity
            .Property(x => x.Received)
            .HasColumnName("received")
            .HasColumnType("timestamp")
            .IsRequired();

        var stateConverter = new ValueConverter<JsonRequestState, int>(
            v => (int)v,
            v => (JsonRequestState)v);

        entity
            .Property(x=>x.State)
            .HasColumnName("state")
            .HasConversion(stateConverter)
            .IsRequired();
    }
}
