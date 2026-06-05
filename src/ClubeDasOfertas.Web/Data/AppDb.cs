using Npgsql;

namespace ClubeDasOfertas.Web.Data;

public sealed class AppDb : IAsyncDisposable
{
    private readonly NpgsqlDataSource _dataSource;

    public AppDb(IConfiguration configuration)
    {
        var configuredConnectionString =
            configuration.GetConnectionString("PostgreSql")
            ?? configuration["POSTGRESQL_CONNECTION"]
            ?? throw new InvalidOperationException("Connection string 'PostgreSql' was not configured.");

        var builder = new NpgsqlConnectionStringBuilder(configuredConnectionString);
        if (string.IsNullOrWhiteSpace(builder.Password))
        {
            var password =
                configuration["ConnectionStrings:PostgreSqlPassword"]
                ?? configuration["POSTGRESQL_PASSWORD"]
                ?? configuration["POSTGRES_PASSWORD"];

            if (!string.IsNullOrWhiteSpace(password))
            {
                builder.Password = password;
            }
        }

        _dataSource = NpgsqlDataSource.Create(builder.ConnectionString);
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
