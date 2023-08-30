internal static class StringExtensions
{
    internal static string ToCamelCase(this string value)
    {
        if (string.IsNullOrEmpty(value) || !char.IsUpper(value[0]))
        {
            return value;
        }

        char[] chars = value.ToCharArray();

        for (int i = 0; i < chars.Length && (i != 1 || char.IsUpper(chars[i])); i++)
        {
            bool flag = i + 1 < chars.Length;
            if (i > 0 && flag && !char.IsUpper(chars[i + 1]))
            {
                if (chars[i + 1] == ' ')
                {
                    chars[i] = char.ToLowerInvariant(chars[i]);
                }
                break;
            }
            chars[i] = char.ToLowerInvariant(chars[i]);
        }

        return new string(chars);
    }
}