namespace JumpRopeAss.Api.Infrastructure;

public sealed class AppOptions
{
    public string? Name { get; init; }
}

public sealed class JwtOptions
{
    public string? Issuer { get; init; }
    public string? Audience { get; init; }
    public string? SigningKey { get; init; }
}

public sealed class MySqlOptions
{
    public string? Host { get; init; }
    public int Port { get; init; } = 3306;
    public string? Database { get; init; }
    public string? User { get; init; }
    public string? Password { get; init; }

    public string ConnectionString
    {
        get
        {
            var host = Host ?? "127.0.0.1";
            var db = Database ?? "jumpropeass_student";
            var user = User ?? "root";
            var pwd = Password ?? string.Empty;
            return $"Server={host};Port={Port};Database={db};User={user};Password={pwd};TreatTinyAsBoolean=true;CharSet=utf8mb4;";
        }
    }
}

