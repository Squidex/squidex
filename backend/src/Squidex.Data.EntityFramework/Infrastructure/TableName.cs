namespace Squidex.Infrastructure;

public static class TableName
{
    private static readonly AsyncLocal<string> Current = new AsyncLocal<string>();

    public static string Prefix
    {
        get => Current.Value ?? string.Empty;
        set => Current.Value = value;
    }
}
