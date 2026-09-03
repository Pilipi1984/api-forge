using System.Text;

namespace ApiForge.Infrastructure.Helpers
{
    /// <summary>
    /// Provides helper methods for converting strings to valid C# identifiers, including PascalCase and camelCase conversions, and handling reserved words.
    /// </summary>
    public static class NameHelper
    {
        private static readonly char[] Separators = { ' ', '/', '{', '}' };

        /// <summary>
        /// A set of C# reserved words that cannot be used as identifiers without escaping. This is used to ensure that generated identifiers do not conflict with C# language keywords.
        /// </summary>
        private static readonly HashSet<string> ReservedWords = new(StringComparer.Ordinal)
        {
            "params", "event", "object", "string", "class", "namespace", "new", "base", "this",
            "public", "private", "protected", "internal", "static", "void", "int", "bool",
            "default", "operator", "readonly", "record", "interface"
        };

        /// <summary>
        /// Converts a given string to PascalCase, removing invalid characters and ensuring it starts with a letter or underscore.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Returns the string converted to PascalCase.</returns>
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
                var cleaned = new string(part.Where(c => char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_').ToArray());
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

        /// <summary>
        /// Converts a given string to camelCase, which is similar to PascalCase but starts with a lowercase letter. 
        /// It removes invalid characters and ensures the first character is lowercase.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Returns the string converted to camelCase.</returns>
        public static string ToCamelCase(string? value)
        {
            var pascal = ToPascalCase(value);
            return char.ToLowerInvariant(pascal[0]) + pascal[1..];
        }

        /// <summary>
        /// Converts a given string to a valid C# identifier. It first converts the string to camelCase and checks if it is a reserved word. 
        /// If it is, it prefixes the identifier with '@' to escape it. If the resulting identifier is empty, it returns a fallback value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="fallback"></param>
        /// <returns>Returns the string converted to a valid C# identifier.</returns>
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