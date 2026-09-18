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
        /// <param name="plan">The solution plan.</param>
        /// <returns>Returns a list of generated files.</returns>
        public static List<GeneratedFile> Generate(SolutionPlan plan)
        {
            var files = new List<GeneratedFile>();
            var ns = plan.Namespaces;

            foreach (var group in plan.Groups)
            {
                var sb = new StringBuilder();
                sb.AppendLine("using System.Threading;");
                sb.AppendLine("using System.Threading.Tasks;");
                sb.AppendLine();
                sb.AppendLine($"using {ns.DomainModelsNamespace};");
                sb.AppendLine();
                sb.AppendLine($"namespace {ns.ApplicationInterfacesNamespace}");
                sb.AppendLine("{");
                sb.AppendLine($"\tpublic interface {group.InterfaceName}");
                sb.AppendLine("\t{");

                foreach (var endpoint in group.Endpoints)
                {
                    if (!string.IsNullOrWhiteSpace(endpoint.Summary))
                    {
                        sb.AppendLine("\t\t/// <summary>");
                        sb.AppendLine($"\t\t/// {endpoint.Summary}");
                        sb.AppendLine("\t\t/// </summary>");
                    }

                    var parameters = EndpointSignatureHelper.BuildParameterList(endpoint);
                    sb.AppendLine($"\t\tTask<{endpoint.ReturnType}> {endpoint.MethodName}Async({parameters}CancellationToken cancellationToken = default);");
                    sb.AppendLine();
                }

                sb.AppendLine("\t}");
                sb.AppendLine("}");

                files.Add(new GeneratedFile
                {
                    RelativePath = GeneratedFilePathHelper.BuildRelativePath(
                        ns.ApplicationNamespace, ns.ApplicationInterfacesNamespace, $"{group.InterfaceName}.cs"),
                    Content = sb.ToString()
                });
            }

            return files;
        }
    }
}