using Npgsql;

namespace ClubeDasOfertas.Web.Data;

public sealed class AppDb : IAsyncDisposable
{
    private readonly NpgsqlDataSource _dataSource;
    private readonly string _connectionTarget;

    public AppDb(IConfiguration configuration)
    {
        var configuredConnectionString =
            configuration.GetConnectionString("PostgreSql")
            ?? configuration["POSTGRESQL_CONNECTION"]
            ?? throw new InvalidOperationException("Connection string 'PostgreSql' was not configured.");

        var builder = new NpgsqlConnectionStringBuilder(configuredConnectionString);
        if (string.Equals(builder.Host, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            builder.Host = "127.0.0.1";
        }

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

        _connectionTarget = $"{builder.Host}:{builder.Port}/{builder.Database}";
        _dataSource = NpgsqlDataSource.Create(builder.ConnectionString);
    }

    public async ValueTask<NpgsqlConnection> OpenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dataSource.OpenConnectionAsync(cancellationToken);
        }
        catch (NpgsqlException ex)
        {
            throw new InvalidOperationException(
                $"Nao foi possivel conectar ao PostgreSQL em {_connectionTarget}. " +
                "Confirme que o banco local esta em execucao e que a senha foi carregada no processo atual antes de iniciar a aplicacao. " +
                "Se estiver usando a configuracao padrao deste repositorio, suba o banco com `docker compose up -d` e carregue as variaveis do arquivo `.env` conforme o README.",
                ex);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _dataSource.DisposeAsync();
    }
}
