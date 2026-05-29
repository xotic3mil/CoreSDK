using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ETL.Infrastructure.SqlGeneration;

internal static class SqlMapper<T>
{
    private static readonly Lazy<Metadata> _meta = new(Build, LazyThreadSafetyMode.ExecutionAndPublication);

    public static string TableName   => _meta.Value.TableName;
    public static string InsertSql   => _meta.Value.InsertSql;
    public static string UpsertSql   => _meta.Value.UpsertSql;
    public static string KeyColumn   => _meta.Value.KeyColumn;
    public static Func<T, object> KeySelector => _meta.Value.KeySelector;

    private sealed class Metadata
    {
        public string TableName  { get; init; } = "";
        public string InsertSql  { get; init; } = "";
        public string UpsertSql  { get; init; } = "";
        public string KeyColumn  { get; init; } = "";
        public Func<T, object> KeySelector { get; init; } = _ =>
            throw new InvalidOperationException($"No [Key] attribute found on {typeof(T).Name}.");
    }

    private static Metadata Build()
    {
        var type      = typeof(T);
        var tableAttr = type.GetCustomAttribute<TableAttribute>();
        var tableName = tableAttr is not null
            ? (tableAttr.Schema is not null ? $"{tableAttr.Schema}.{tableAttr.Name}" : tableAttr.Name)
            : ToSnakeCase(type.Name);

        var props = type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.GetCustomAttribute<NotMappedAttribute>() is null)
            .Select(p => new
            {
                Property = p,
                Column   = p.GetCustomAttribute<ColumnAttribute>()?.Name ?? ToSnakeCase(p.Name),
                IsKey    = p.GetCustomAttribute<KeyAttribute>() is not null,
            })
            .ToArray();

        if (props.Length == 0)
            throw new InvalidOperationException($"Type {type.Name} has no mappable properties.");

        var cols     = string.Join(", ", props.Select(p => p.Column));
        var parms    = string.Join(", ", props.Select(p => $"@{p.Property.Name}"));
        var insertSql = $"INSERT INTO {tableName} ({cols}) VALUES ({parms})";

        var keyProp   = props.FirstOrDefault(p => p.IsKey);
        var keyColumn = keyProp?.Column ?? "";

        Func<T, object> keySelector = _ =>
            throw new InvalidOperationException($"No [Key] attribute found on {type.Name}.");

        var upsertSql = "";

        if (keyProp is not null)
        {
            var nonKey    = props.Where(p => !p.IsKey).ToArray();
            var conflictAction = nonKey.Length > 0
                ? $"DO UPDATE SET {string.Join(", ", nonKey.Select(p => $"{p.Column} = EXCLUDED.{p.Column}"))}"
                : "DO NOTHING";
            upsertSql = $"INSERT INTO {tableName} ({cols}) VALUES ({parms}) ON CONFLICT ({keyColumn}) {conflictAction}";

            var propInfo = keyProp.Property;
            keySelector  = t => propInfo.GetValue(t)!;
        }

        return new Metadata
        {
            TableName   = tableName,
            InsertSql   = insertSql,
            UpsertSql   = upsertSql,
            KeyColumn   = keyColumn,
            KeySelector = keySelector,
        };
    }

    private static string ToSnakeCase(string name) =>
        Regex.Replace(name, @"([a-z0-9])([A-Z])", "$1_$2").ToLowerInvariant();
}
