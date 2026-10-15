namespace ApiForge.Infrastructure.Helpers
{
    /// <summary>
    /// Separa un nombre de esquema potencialmente cualificado (p. ej. proveniente de
    /// namespaces de otro proyecto: "Company.Product.Layer.Folder.ClassName") en el
    /// nombre de clase real y los segmentos de namespace/carpeta que lo preceden.
    /// </summary>
    internal static class QualifiedNameResolver
    {
        /// <summary>
        /// Represents the resolved name of a schema, including the class name and namespace segments.
        /// </summary>
        public sealed record ResolvedName
        {
            /// <summary>
            /// Gets the resolved class name.
            /// </summary>
            public required string ClassName { get; init; }
            /// <summary>
            /// Gets the list of namespace segments that precede the class name.
            /// </summary>
            public required IReadOnlyList<string> NamespaceSegments { get; init; }
        }

        /// <summary>
        /// Resolves a potentially qualified schema name into its class name and namespace segments.
        /// </summary>
        /// <param name="fullName">The potentially qualified schema name.</param>
        /// <returns>The resolved name.</returns>
        public static ResolvedName Resolve(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                return new ResolvedName { ClassName = "Unnamed", NamespaceSegments = [] };
            }

            var rawSegments = fullName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (rawSegments.Length == 0)
            {
                return new ResolvedName { ClassName = "Unnamed", NamespaceSegments = [] };
            }

            // Each segment is already separated by '.', so NameHelper.ToPascalCase only needs to normalize internal characters (hyphens, spaces, etc.)
            // without merging segments together.
            var className = NameHelper.ToPascalCase(rawSegments[^1]);
            var namespaceSegments = rawSegments
                .Take(rawSegments.Length - 1)
                .Select(NameHelper.ToPascalCase)
                .ToList();

            return new ResolvedName { ClassName = className, NamespaceSegments = namespaceSegments };
        }

        /// <summary>
        /// Builds a namespace string by combining the base namespace with the provided namespace segments.
        /// </summary>
        /// <param name="baseNamespace">The base namespace.</param>
        /// <param name="namespaceSegments">The namespace segments.</param>
        /// <returns>The combined namespace string.</returns>
        public static string BuildNamespace(string baseNamespace, IReadOnlyList<string> namespaceSegments) =>
            namespaceSegments.Count == 0
                ? baseNamespace
                : $"{baseNamespace}.{string.Join('.', namespaceSegments)}";

        /// <summary>
        /// Builds a folder path string by joining the provided namespace segments with '/' and prepending a leading '/'.
        /// </summary>
        /// <param name="namespaceSegments">The namespace segments.</param>
        /// <returns>The combined folder path string.</returns>
        public static string BuildFolderPath(IReadOnlyList<string> namespaceSegments) =>
            namespaceSegments.Count == 0
                ? string.Empty
                : "/" + string.Join('/', namespaceSegments);
    }
}