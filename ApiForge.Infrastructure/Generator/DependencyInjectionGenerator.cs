using ApiForge.Domain.GeneratedApiSolution;
using ApiForge.Infrastructure.Generator.Planning;
using ApiForge.Infrastructure.Helpers;
using System.Text;

namespace ApiForge.Infrastructure.Generator
{
    /// <summary>
    /// Generates the dependency injection extension method for registering API clients in the service collection, based on the provided solution plan.
    /// </summary>
    public static class DependencyInjectionGenerator
    {
        /// <summary>
        /// Generates the dependency injection extension method for registering API clients in the service collection, based on the provided solution plan.
        /// </summary>
        /// <param name="plan">The solution plan.</param>
        /// <returns>Returns the generated dependency injection file.</returns>
        public static GeneratedFile Generate(SolutionPlan plan)
        {
            var ns = plan.Namespaces;

            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            sb.AppendLine($"using {ns.ApplicationInterfacesNamespace};");
            sb.AppendLine($"using {ns.InfrastructureClientsNamespace};");
            sb.AppendLine();
            sb.AppendLine($"namespace {ns.InfrastructureDependencyInjectionNamespace}");
            sb.AppendLine("{");
            sb.AppendLine("    public static class ServiceCollectionExtensions");
            sb.AppendLine("    {");
            sb.AppendLine($"        public static IServiceCollection {ns.ClientsExtensionMethodName}(this IServiceCollection services, Uri baseAddress)");
            sb.AppendLine("        {");

            foreach (var group in plan.Groups)
            {
                sb.AppendLine($"            services.AddHttpClient<{group.InterfaceName}, {group.ClassName}>(client =>");
                sb.AppendLine("            {");
                sb.AppendLine("                client.BaseAddress = baseAddress;");
                sb.AppendLine("            });");
            }

            sb.AppendLine();
            sb.AppendLine("            return services;");
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");

            return new GeneratedFile
            {
                RelativePath = GeneratedFilePathHelper.BuildRelativePath(
                    ns.InfrastructureNamespace, ns.InfrastructureDependencyInjectionNamespace, "ServiceCollectionExtensions.cs"),
                Content = sb.ToString()
            };
        }
    }
}