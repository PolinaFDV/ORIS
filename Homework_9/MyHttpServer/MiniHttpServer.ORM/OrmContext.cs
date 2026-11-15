using System.Reflection;
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

    public async Task<IReadOnlyList<T>> GetAllAsync<T>(string tableName, CancellationToken cancellationToken = default) where T : class, new()
    {
        string safeTableName = EscapeIdentifier(tableName);
        string sql = $"SELECT * FROM {safeTableName}";
        var result = new List<T>();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite && p.SetMethod?.IsPublic == true).ToArray();
        var columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < reader.FieldCount; i++) 
        {
            columns[reader.GetName(i).ToLowerInvariant()] = i;
        }

        while (await reader.ReadAsync(cancellationToken))
        {
            T entity = new();
            foreach (PropertyInfo property in properties)
            {
                if (!columns.TryGetValue(property.Name.ToLowerInvariant(), out int ordinal)) continue;
                if (await reader.IsDBNullAsync(ordinal, cancellationToken))
                {
                    if (!property.PropertyType.IsValueType || Nullable.GetUnderlyingType(property.PropertyType) != null)
                        property.SetValue(entity, null);
                    continue;
                }
                object value = reader.GetValue(ordinal);
                Type targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                object convertedValue = targetType.IsInstanceOfType(value) ? value : 
                    targetType.IsEnum ? Enum.ToObject(targetType, value) : Convert.ChangeType(value, targetType);
                property.SetValue(entity, convertedValue);
            }
            result.Add(entity);
        }
        return result;
    }

    public async Task<int> InsertAsync<T>(string tableName, T entity, CancellationToken cancellationToken = default) where T : class
    {
        string safeTableName = EscapeIdentifier(tableName);
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.Name != "Id" && p.CanRead).ToArray();
        
        string columns = string.Join(", ", properties.Select(p => EscapeIdentifier(p.Name)));
        string parameters = string.Join(", ", properties.Select(p => "@" + p.Name));
        string sql = $"INSERT INTO {safeTableName} ({columns}) VALUES ({parameters})";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        foreach (var prop in properties)
            command.Parameters.AddWithValue("@" + prop.Name, prop.GetValue(entity) ?? DBNull.Value);
        
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> UpdateAsync<T>(string tableName, int id, T entity, CancellationToken cancellationToken = default) where T : class
    {
        string safeTableName = EscapeIdentifier(tableName);
        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.Name != "Id" && p.CanRead).ToArray();
        
        string setClause = string.Join(", ", properties.Select(p => $"{EscapeIdentifier(p.Name)} = @{p.Name}"));
        string sql = $"UPDATE {safeTableName} SET {setClause} WHERE Id = @Id";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        foreach (var prop in properties)
            command.Parameters.AddWithValue("@" + prop.Name, prop.GetValue(entity) ?? DBNull.Value);
        command.Parameters.AddWithValue("@Id", id);
        
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<int> DeleteAsync(string tableName, int id, CancellationToken cancellationToken = default)
    {
        string safeTableName = EscapeIdentifier(tableName);
        string sql = $"DELETE FROM {safeTableName} WHERE Id = @Id";

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("@Id", id);
        
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static string EscapeIdentifier(string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier) || !Regex.IsMatch(identifier, @"^[A-Za-z_][A-Za-z0-9_]*$"))
            throw new ArgumentException("Недопустимое имя SQL-идентификатора.", nameof(identifier));
        return $"\"{identifier}\"";
    }
}