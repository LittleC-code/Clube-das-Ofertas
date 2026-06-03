using Npgsql;

namespace ClubeDasOfertas.Web.Data;

public sealed class AppDb : IAsyncDisposable
{
    private readonly NpgsqlDataSource _dataSource;

    public AppDb(IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("PostgreSql")
            ?? configuration["POSTGRESQL_CONNECTION"]
            ?? throw new InvalidOperationException("Connection string 'PostgreSql' was not configured.");

        _dataSource = NpgsqlDataSource.Create(connectionString);
    }

    public ValueTask<NpgsqlConnection> OpenAsync(CancellationToken cancellationToken = default)
    {
        return _dataSource.OpenConnectionAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _dataSource.DisposeAsync();
    }
}
