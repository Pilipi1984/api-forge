using ApiForge.Domain.GeneratedApiSolution;
using ApiForge.Domain.Models;
using ApiForge.Infrastructure.Generator.Resolvers;
using ApiForge.Infrastructure.Helpers;
using System.Text;

namespace ApiForge.Infrastructure.Generator
{
    public static class DomainModelGenerator
    {
        public static List<GeneratedFile> Generate(ApiDefinition definition, string rootNamespace)
        {
            var files = new List<GeneratedFile>();
            var modelsNamespace = $"{rootNamespace}.Domain.Models";

            foreach (var model in definition.Models)
            {
                var className = NameHelper.ToPascalCase(model.Name);
                var usedNames = new HashSet<string>(StringComparer.Ordinal) { className };

                var sb = new StringBuilder();
                sb.AppendLine($"namespace {modelsNamespace}");
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
                    RelativePath = $"{rootNamespace}.Domain/Models/{className}.cs",
                    Content = sb.ToString()
                });
            }

            return files;
        }

        private static string MakeUniquePropertyName(string candidate, HashSet<string> used)
        {
            var name = candidate;
            var i = 1;
            while (!used.Add(name))
                name = candidate + i++;
            return name;
        }

        private static string GetDefaultAssignment(string type) =>
            type == "string" ? " = string.Empty;" : string.Empty;

        private static string EscapeXmlComment(string value) =>
            value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
    }
}