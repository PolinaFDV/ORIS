using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Npgsql;

namespace MiniHttpServer.ORM;

public sealed class OrmContext : IOrmContext
{
    private readonly string _connectionString;

    public OrmContext(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        _connectionString = connectionString;
    }

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(string tableName, CancellationToken ct = default) where T : class, new()
    {
        string safeTableName = SqlBuilder.EscapeIdentifier(tableName);
        string sql = $"SELECT * FROM {safeTableName}";
        return await ExecuteSelectAsync<T>(sql, null, ct);
    }

    public async Task<T?> GetByIdAsync<T>(string tableName, int id, CancellationToken ct = default) where T : class, new()
    {
        string safeTableName = SqlBuilder.EscapeIdentifier(tableName);
        string sql = $"SELECT * FROM {safeTableName} WHERE \"Id\" = @Id";
        var result = await ExecuteSelectAsync<T>(sql, cmd => cmd.Parameters.AddWithValue("@Id", id), ct);
        return result.FirstOrDefault();
    }

    public async Task<int> InsertAsync<T>(string tableName, T entity, CancellationToken ct = default) where T : class
    {
        var (sql, parameters) = SqlBuilder.BuildInsert(tableName, entity);
        return await ExecuteNonQueryAsync(sql, parameters, ct);
    }

    public async Task<int> UpdateAsync<T>(string tableName, int id, T entity, CancellationToken ct = default) where T : class
    {
        var (sql, parameters) = SqlBuilder.BuildUpdate(tableName, id, entity);
        return await ExecuteNonQueryAsync(sql, parameters, ct);
    }

    public async Task<int> DeleteAsync(string tableName, int id, CancellationToken ct = default)
    {
        var (sql, parameters) = SqlBuilder.BuildDelete(tableName, id);
        return await ExecuteNonQueryAsync(sql, parameters, ct);
    }

    private async Task<IReadOnlyList<T>> ExecuteSelectAsync<T>(string sql, Action<NpgsqlCommand>? configure, CancellationToken ct) where T : class, new()
    {
        var result = new List<T>();
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(ct);
        await using var command = new NpgsqlCommand(sql, connection);
        configure?.Invoke(command);
        await using var reader = await command.ExecuteReaderAsync(ct);

        PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite && p.SetMethod?.IsPublic == true).ToArray();

        var columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < reader.FieldCount; i++) columns[reader.GetName(i)] = i;

        while (await reader.ReadAsync(ct))
        {
            T entity = new();
            foreach (PropertyInfo property in properties)
            {
                if (!columns.TryGetValue(property.Name, out int ordinal)) continue;

                if (await reader.IsDBNullAsync(ordinal, ct))
                {
                    if (!property.PropertyType.IsValueType || Nullable.GetUnderlyingType(property.PropertyType) != null)
                        property.SetValue(entity, null);
                    continue;
                }

                object value = reader.GetValue(ordinal);
                Type targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                object converted = targetType.IsInstanceOfType(value) ? value :
                    targetType.IsEnum ? Enum.ToObject(targetType, value) :
                    Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);

                property.SetValue(entity, converted);
            }
            result.Add(entity);
        }
        return result;
    }

    private async Task<int> ExecuteNonQueryAsync(string sql, Dictionary<string, object?> parameters, CancellationToken ct)
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(ct);
        await using var command = new NpgsqlCommand(sql, connection);
        foreach (var p in parameters) command.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
        return await command.ExecuteNonQueryAsync(ct);
    }
}

public static class SqlBuilder
{
    public static string EscapeIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier) || !Regex.IsMatch(identifier, @"^[A-Za-z_][A-Za-z0-9_]*$"))
            throw new ArgumentException("Недопустимое имя SQL-идентификатора.", nameof(identifier));
        return $"\"{identifier}\"";
    }

    public static (string Sql, Dictionary<string, object?> Parameters) BuildInsert<T>(string tableName, T entity) where T : class
    {
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.Name != "Id" && p.CanRead).ToArray();

        var sql = new StringBuilder($"INSERT INTO {EscapeIdentifier(tableName)} (");
        sql.Append(string.Join(", ", props.Select(p => EscapeIdentifier(p.Name))));
        sql.Append(") VALUES (");
        sql.Append(string.Join(", ", props.Select(p => "@" + p.Name)));
        sql.Append(");");

        var parameters = new Dictionary<string, object?>();
        foreach (var prop in props) parameters.Add("@" + prop.Name, prop.GetValue(entity));
        return (sql.ToString(), parameters);
    }

    public static (string Sql, Dictionary<string, object?> Parameters) BuildUpdate<T>(string tableName, int id, T entity) where T : class
    {
        var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.Name != "Id" && p.CanRead).ToArray();

        var sql = new StringBuilder($"UPDATE {EscapeIdentifier(tableName)} SET ");
        sql.Append(string.Join(", ", props.Select(p => $"{EscapeIdentifier(p.Name)} = @{p.Name}")));
        sql.Append(" WHERE \"Id\" = @Id;");

        var parameters = new Dictionary<string, object?>();
        foreach (var prop in props) parameters.Add("@" + prop.Name, prop.GetValue(entity));
        parameters.Add("@Id", id);
        return (sql.ToString(), parameters);
    }

    public static (string Sql, Dictionary<string, object?> Parameters) BuildDelete(string tableName, int id)
    {
        string sql = $"DELETE FROM {EscapeIdentifier(tableName)} WHERE \"Id\" = @Id;";
        return (sql, new Dictionary<string, object?> { { "@Id", id } });
    }
}