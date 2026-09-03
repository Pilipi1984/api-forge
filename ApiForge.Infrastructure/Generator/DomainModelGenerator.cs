using ApiForge.Domain.GeneratedApiSolution;
using ApiForge.Domain.Models;
using ApiForge.Infrastructure.Generator.Resolvers;
using ApiForge.Infrastructure.Helpers;
using System.Text;

namespace ApiForge.Infrastructure.Generator
{
    /// <summary>
    /// Generates domain model classes based on the provided API definition.
    /// </summary>
    public static class DomainModelGenerator
    {
        /// <summary>
        /// Generates domain model classes based on the provided API definition.
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="rootNamespace"></param>
        /// <returns>Returns a list of generated files.</returns>
        public static List<GeneratedFile> Generate(ApiDefinition definition, string rootNamespace)
        {
            var files = new List<GeneratedFile>();
            var modelsRootNamespace = $"{rootNamespace}.Domain.Models";

            foreach (var model in definition.Models)
            {
                var resolved = QualifiedNameResolver.Resolve(model.Name);
                var className = resolved.ClassName;
                var modelNamespace = QualifiedNameResolver.BuildNamespace(modelsRootNamespace, resolved.NamespaceSegments);
                var folderSuffix = QualifiedNameResolver.BuildFolderPath(resolved.NamespaceSegments);

                var usedNames = new HashSet<string>(StringComparer.Ordinal) { className };

                var sb = new StringBuilder();
                sb.AppendLine($"namespace {modelNamespace}");
                sb.AppendLine("{");
                sb.AppendLine($"    public class {className}");
                sb.AppendLine("    {");

                foreach (var property in model.Properties)
                {
                    var propName = MakeUniquePropertyName(NameHelper.ToPascalCase(property.Name), usedNames);
                    var propType = CSharpTypeResolver.ResolveProperty(property);

                    if (!string.IsNullOrWhiteSpace(property.Description))
                    {
                        sb.AppendLine("        /// <summary>");
                        sb.AppendLine($"        /// {EscapeXmlComment(property.Description)}");
                        sb.AppendLine("        /// </summary>");
                    }

                    sb.AppendLine($"        public {propType} {propName} {{ get; set; }}{GetDefaultAssignment(propType)}");
                    sb.AppendLine();
                }

                sb.AppendLine("    }");
                sb.AppendLine("}");

                files.Add(new GeneratedFile
                {
                    RelativePath = $"{rootNamespace}.Domain/Models{folderSuffix}/{className}.cs",
                    Content = sb.ToString()
                });
            }

            return files;
        }

        /// <summary>
        /// Generates a unique property name by appending a number if the candidate name is already used.
        /// </summary>
        /// <param name="candidate"></param>
        /// <param name="used"></param>
        /// <returns>Returns a unique property name.</returns>
        private static string MakeUniquePropertyName(string candidate, HashSet<string> used)
        {
            var name = candidate;
            var i = 1;
            while (!used.Add(name))
                name = candidate + i++;
            return name;
        }

        /// <summary>
        /// Returns a default assignment string for a given type. For string types, it returns " = string.Empty;", otherwise it returns an empty string.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Returns a default assignment string.</returns>
        private static string GetDefaultAssignment(string type) =>
            type == "string" ? " = string.Empty;" : string.Empty;

        /// <summary>
        /// Escapes special characters in XML comments to ensure they are valid XML. Specifically, it replaces &, <, and > with their corresponding XML entities.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Returns the escaped XML comment.</returns>
        private static string EscapeXmlComment(string value) =>
            value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
    }
}