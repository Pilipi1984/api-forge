using ApiForge.Domain.GeneratedApiSolution;
using ApiForge.Infrastructure.Generator.Planning;
using ApiForge.Infrastructure.Helpers;
using System.Text;

namespace ApiForge.Infrastructure.Generator
{
    /// <summary>
    /// Generates C# interface files for API contracts based on the provided solution plan, 
    /// creating an interface for each group of endpoints with methods corresponding to the defined endpoints.
    /// </summary>
    public static class ApiContractGenerator
    {
        /// <summary>
        /// Generates C# interface files for API contracts based on the provided solution plan.
        /// </summary>
        /// <param name="plan"></param>
        /// <returns>Returns a list of generated files.</returns>
        public static List<GeneratedFile> Generate(SolutionPlan plan)
        {
            var files = new List<GeneratedFile>();
            var modelsNamespace = $"{plan.RootNamespace}.Domain.Models";
            var interfacesNamespace = $"{plan.RootNamespace}.Application.Interfaces";

            foreach (var group in plan.Groups)
            {
                var sb = new StringBuilder();
                sb.AppendLine("using System.Threading;");
                sb.AppendLine("using System.Threading.Tasks;");
                sb.AppendLine($"using {modelsNamespace};");
                sb.AppendLine();
                sb.AppendLine($"namespace {interfacesNamespace}");
                sb.AppendLine("{");
                sb.AppendLine($"    public interface {group.InterfaceName}");
                sb.AppendLine("    {");

                foreach (var endpoint in group.Endpoints)
                {
                    if (!string.IsNullOrWhiteSpace(endpoint.Summary))
                    {
                        sb.AppendLine("        /// <summary>");
                        sb.AppendLine($"        /// {endpoint.Summary}");
                        sb.AppendLine("        /// </summary>");
                    }

                    var parameters = EndpointSignatureHelper.BuildParameterList(endpoint);
                    sb.AppendLine($"        Task<{endpoint.ReturnType}> {endpoint.MethodName}Async({parameters}CancellationToken cancellationToken = default);");
                    sb.AppendLine();
                }

                sb.AppendLine("    }");
                sb.AppendLine("}");

                files.Add(new GeneratedFile
                {
                    RelativePath = $"{plan.RootNamespace}.Application/Interfaces/{group.InterfaceName}.cs",
                    Content = sb.ToString()
                });
            }

            return files;
        }
    }
}