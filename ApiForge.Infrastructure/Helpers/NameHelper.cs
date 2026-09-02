using System.Text;

namespace ApiForge.Infrastructure.Helpers
{
    public static class NameHelper
    {
        private static readonly char[] Separators = { ' ', '_', '-', '.', '/', '{', '}' };

        private static readonly HashSet<string> ReservedWords = new(StringComparer.Ordinal)
        {
            "params", "event", "object", "string", "class", "namespace", "new", "base", "this",
            "public", "private", "protected", "internal", "static", "void", "int", "bool",
            "default", "operator", "readonly", "record", "interface"
        };

        public static string ToPascalCase(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Unnamed";
            }

            var parts = value.Split(Separators, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
            {
                return "Unnamed";
            }

            var sb = new StringBuilder();
            foreach (var part in parts)
            {
                var cleaned = new string(part.Where(char.IsLetterOrDigit).ToArray());
                if (cleaned.Length == 0)
                {
                    continue;
                }

                sb.Append(char.ToUpperInvariant(cleaned[0]));
                if (cleaned.Length > 1)
                {
                    sb.Append(cleaned[1..]);
                }
            }

            var result = sb.ToString();
            if (result.Length == 0)
            {
                return "Unnamed";
            }

            if (char.IsDigit(result[0]))
            {
                result = "_" + result;
            }

            return result;
        }

        public static string ToCamelCase(string? value)
        {
            var pascal = ToPascalCase(value);
            return char.ToLowerInvariant(pascal[0]) + pascal[1..];
        }

        public static string ToValidIdentifier(string? value, string fallback = "value")
        {
            var candidate = ToCamelCase(value);
            if (string.IsNullOrEmpty(candidate)) 
            { 
                return fallback;
            }

            return ReservedWords.Contains(candidate) ? "@" + candidate : candidate;
        }
    }
}