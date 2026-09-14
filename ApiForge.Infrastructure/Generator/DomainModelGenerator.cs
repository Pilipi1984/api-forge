using ApiForge.Domain.GeneratedApiSolution;
using ApiForge.Domain.Models;
using ApiForge.Infrastructure.Generator.Planning;
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
        /// Generates domain model classes based on the provided API definition and project namespaces.
        /// </summary>
        /// <param name="definition">The API definition.</param>
        /// <param name="ns">The project namespaces.</param>
        /// <returns>Returns a list of generated files.</returns>
        public static List<GeneratedFile> Generate(ApiDefinition definition, ProjectNamespaces ns)
        {
            var files = new List<GeneratedFile>();

            foreach (var model in definition.Models)
            {
                var name = model.Name.Contains(ns.DomainNamespace) ? model.Name.Substring(ns.DomainNamespace.Length + 2) : model.Name;

                var className = NameHelper.ToPascalCase(name);
                var usedNames = new HashSet<string>(StringComparer.Ordinal) { className };

                var sb = new StringBuilder();
                sb.AppendLine($"namespace {ns.DomainModelsNamespace}");
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
                    RelativePath = GeneratedFilePathHelper.BuildRelativePath(
                        ns.DomainNamespace, ns.DomainModelsNamespace, $"{className}.cs"),
                    Content = sb.ToString()
                });
            }

            return files;
        }
        /// <summary>
        /// Generates a unique property name by appending a number if the candidate name is already used.
        /// </summary>
        /// <param name="candidate">The candidate property name.</param>
        /// <param name="used">The set of already used property names.</param>
        /// <returns>The unique property name.</returns>
        private static string MakeUniquePropertyName(string candidate, HashSet<string> used)
        {
            var name = candidate;
            var i = 1;
            while (!used.Add(name))
            {
                name = candidate + i++;
            }

            return name;
        }

        private static string GetDefaultAssignment(string type) =>
            type == "string" ? " = string.Empty;" : string.Empty;

        private static string EscapeXmlComment(string value) =>
            value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
    }
}