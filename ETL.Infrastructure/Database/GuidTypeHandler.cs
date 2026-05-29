using System.Data;
using Dapper;

namespace ETL.Infrastructure.Database;

public sealed class GuidTypeHandler : SqlMapper.TypeHandler<Guid>
{
    public static readonly GuidTypeHandler Instance = new();

    public override Guid Parse(object value) => Guid.Parse((string)value);

    public override void SetValue(IDbDataParameter parameter, Guid value)
        => parameter.Value = value.ToString();
}
