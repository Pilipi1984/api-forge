using ApiForge.Domain.GeneratedApiSolution;
using ApiForge.Infrastructure.Generator.Planning;
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
        /// <param name="plan"></param>
        /// <returns>Returns the generated dependency injection file.</returns>
        public static GeneratedFile Generate(SolutionPlan plan)
        {
            var interfacesNamespace = $"{plan.RootNamespace}.Application.Interfaces";
            var clientsNamespace = $"{plan.RootNamespace}.Infrastructure.Clients";
            var extensionMethodName = $"Add{plan.RootNamespace}Clients";

            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            sb.AppendLine($"using {interfacesNamespace};");
            sb.AppendLine($"using {clientsNamespace};");
            sb.AppendLine();
            sb.AppendLine($"namespace {plan.RootNamespace}.Infrastructure.DependencyInjection");
            sb.AppendLine("{");
            sb.AppendLine("    public static class ServiceCollectionExtensions");
            sb.AppendLine("    {");
            sb.AppendLine($"        public static IServiceCollection {extensionMethodName}(this IServiceCollection services, Uri baseAddress)");
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
                RelativePath = $"{plan.RootNamespace}.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs",
                Content = sb.ToString()
            };
        }
    }
}