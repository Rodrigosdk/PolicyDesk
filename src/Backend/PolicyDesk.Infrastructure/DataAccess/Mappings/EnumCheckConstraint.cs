namespace PolicyDesk.Infrastructure.DataAccess.Mappings;

internal static class EnumCheckConstraint
{
    public static string AllowedValues<TEnum>(string column) where TEnum : struct, Enum =>
        $"{column} IN ({string.Join(", ", Enum.GetNames<TEnum>().Select(name => $"'{name}'"))})";
}
