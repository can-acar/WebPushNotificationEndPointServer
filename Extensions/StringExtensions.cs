namespace Extensions;

public static class StringExtensions
{
    public static string FirstLower(this string str)
    {
        if (!string.IsNullOrEmpty(str) && str!.Length > 1) return char.ToLowerInvariant(str[0]) + str.Substring(1);

        return str;
    }

    public static string FirstUpper(this string str)
    {
        if (!string.IsNullOrEmpty(str) && str!.Length > 1) return char.ToUpperInvariant(str[0]) + str.Substring(1);

        return str;
    }

    public static int ToInt(this string value)
    {
        if (int.TryParse(value, out var result)) return result;

        return 0;
    }
}