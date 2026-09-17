using ApiForge.Domain.Enums;

namespace ApiForge.Infrastructure.Helpers
{
    /// <summary>
    /// Parses a free-text architecture style value (coming either from the OpenAPI spec's
    /// "x-architecture" vendor extension or from an explicit user selection in the UI) into an
    /// <see cref="ArchitectureStyle"/>. Single source of truth for the accepted string values,
    /// so the spec-driven detection and the UI override stay consistent.
    /// </summary>
    public static class ArchitectureStyleParser
    {
        /// <summary>
        /// Attempts to parse the given value into an <see cref="ArchitectureStyle"/>.
        /// Accepted values (case-insensitive): "hexagonal" / "ports-and-adapters" for Hexagonal,
        /// "clean" / "clean-architecture" for Clean Architecture.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="style">The parsed style, or <see cref="ArchitectureStyle.CleanArchitecture"/> if parsing failed.</param>
        /// <returns>True if the value matched a known architecture style; otherwise false.</returns>
        public static bool TryParse(string? value, out ArchitectureStyle style)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                var trimmed = value.Trim();

                if (string.Equals(trimmed, "hexagonal", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(trimmed, "ports-and-adapters", StringComparison.OrdinalIgnoreCase))
                {
                    style = ArchitectureStyle.Hexagonal;
                    return true;
                }

                if (string.Equals(trimmed, "clean", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(trimmed, "clean-architecture", StringComparison.OrdinalIgnoreCase))
                {
                    style = ArchitectureStyle.CleanArchitecture;
                    return true;
                }
            }

            style = ArchitectureStyle.CleanArchitecture;
            return false;
        }
    }
}
