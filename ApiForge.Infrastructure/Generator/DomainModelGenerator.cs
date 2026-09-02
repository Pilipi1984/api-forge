using ApiForge.Domain.GeneratedApiSolution;
using ApiForge.Domain.Models;
using ApiForge.Infrastructure.Generator.Resolvers;
using ApiForge.Infrastructure.Helpers;
using System.Text;

namespace ApiForge.Infrastructure.Generator
{
    /// <summary>
    /// Generates C# domain model classes based on the provided API definition, creating a class for each model with properties corresponding to the model's schema.
    /// </summary>
    public static class DomainModelGenerator
    {
        /// <summary>
        /// Generates C# domain model classes based on the provided API definition.
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="rootNamespace"></param>
        /// <returns>Returns a list of generated files.</returns>
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

        /// <summary>
        /// Generates a unique property name by appending a number if the candidate name has already been used. 
        /// This ensures that all property names in a class are unique, even if the original names are the same after formatting.
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
        /// Returns a default assignment string for a given type. For strings, it returns " = string.Empty;", while for other types, it returns an empty string. 
        /// This is used to initialize properties with default values in the generated classes.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>Returns a default assignment string.</returns>
        private static string GetDefaultAssignment(string type) =>
            type == "string" ? " = string.Empty;" : string.Empty;

        /// <summary>
        /// Escapes special characters in XML comments to ensure that the generated documentation is valid. It replaces &, <, and > with their corresponding XML entities.
        /// </summary>
        /// <param name="value"></param>
        /// <returns>Returns the escaped XML comment.</returns>
        private static string EscapeXmlComment(string value) =>
            value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
    }
}