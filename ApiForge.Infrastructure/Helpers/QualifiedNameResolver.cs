namespace ApiForge.Infrastructure.Helpers
{
    /// <summary>
    /// Separa un nombre de esquema potencialmente cualificado (p. ej. proveniente de
    /// namespaces de otro proyecto: "Company.Product.Layer.Folder.ClassName") en el
    /// nombre de clase real y los segmentos de namespace/carpeta que lo preceden.
    /// </summary>
    internal static class QualifiedNameResolver
    {
        public sealed record ResolvedName
        {
            public required string ClassName { get; init; }
            public required IReadOnlyList<string> NamespaceSegments { get; init; }
        }

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

        public static string BuildNamespace(string baseNamespace, IReadOnlyList<string> namespaceSegments) =>
            namespaceSegments.Count == 0
                ? baseNamespace
                : $"{baseNamespace}.{string.Join('.', namespaceSegments)}";

        public static string BuildFolderPath(IReadOnlyList<string> namespaceSegments) =>
            namespaceSegments.Count == 0
                ? string.Empty
                : "/" + string.Join('/', namespaceSegments);
    }
}