namespace ApiForge.Infrastructure.Helpers
{
    /// <summary>
    /// Provides the relative path of a generated file based on the root namespace of the project and the full namespace of the file.
    /// - root namespace of the project (e.g., "{Root}.Domain") defines the project folder.
    /// - full namespace of the file (e.g., "{Root}.Domain.Models") determines subfolders relative to the project folder.
    /// No generator invents subfolders that do not correspond to the actual namespace, and the project suffix is not duplicated in multiple places.
    /// </summary>
    internal static class GeneratedFilePathHelper
    {
        /// <summary>
        /// Builds the relative path of a generated file based on the project namespace, full namespace, and file name.
        /// </summary>
        /// <param name="projectNamespace">The root namespace of the project.</param>
        /// <param name="fullNamespace">The full namespace of the file.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The relative path of the generated file.</returns>
        public static string BuildRelativePath(string projectNamespace, string fullNamespace, string fileName)
        {
            var subFolder = GetSubFolder(projectNamespace, fullNamespace);

            return string.IsNullOrEmpty(subFolder)
                ? $"{projectNamespace}/{fileName}"
                : $"{projectNamespace}/{subFolder}/{fileName}";
        }

        /// <summary>
        /// Gets the subfolder path based on the project namespace and full namespace.
        /// </summary>
        /// <param name="projectNamespace">The root namespace of the project.</param>
        /// <param name="fullNamespace">The full namespace of the file.</param>
        /// <returns>The subfolder path.</returns>
        private static string GetSubFolder(string projectNamespace, string fullNamespace)
        {
            if (string.Equals(projectNamespace, fullNamespace, StringComparison.Ordinal))
            {
                return string.Empty;
            }

            var prefix = projectNamespace + ".";
            if (!fullNamespace.StartsWith(prefix, StringComparison.Ordinal))
            {
                // No cuelga del namespace del proyecto: no inventamos jerarquía de carpetas.
                return string.Empty;
            }

            return fullNamespace[prefix.Length..].Replace('.', '/');
        }
    }
}