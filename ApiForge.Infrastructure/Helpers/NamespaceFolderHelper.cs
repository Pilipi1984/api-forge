namespace ApiForge.Infrastructure.Helpers
{
    internal static class NamespaceFolderHelper
    {
        public static string ToRelativeFolder(string namespaceSuffix, string projectSegment)
        {
            var trimmed = namespaceSuffix.StartsWith(projectSegment + ".", StringComparison.Ordinal)
                ? namespaceSuffix[(projectSegment.Length + 1)..]
                : namespaceSuffix;

            return trimmed.Replace('.', '/');
        }
    }
}