using ApiForge.Domain.GeneratedApiSolution;
using ApiForge.Infrastructure.Generator.Planning;
using ApiForge.Infrastructure.Helpers;
using System.Text;

namespace ApiForge.Infrastructure.Generator
{
    /// <summary>
    /// Generates the Program.cs file for the generated API client, including service registration and a placeholder for the base address configuration.
    /// </summary>
    public static class ProgramGenerator
    {
        /// <summary>
        /// Generates the Program.cs file for the generated API client, including service registration and a placeholder for the base address configuration.
        /// </summary>
        /// <param name="plan"></param>
        /// <returns>Returns the generated Program.cs file.</returns>
        public static GeneratedFile Generate(SolutionPlan plan)
        {
            var ns = plan.Namespaces;

            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            sb.AppendLine($"using {ns.InfrastructureDependencyInjectionNamespace};");
            sb.AppendLine();
            sb.AppendLine("var services = new ServiceCollection();");
            sb.AppendLine($"services.{ns.ClientsExtensionMethodName}(new Uri(\"https://localhost\"));");
            sb.AppendLine();
            sb.AppendLine("using var provider = services.BuildServiceProvider();");
            sb.AppendLine();
            sb.AppendLine("Console.WriteLine(\"Cliente de API generado y listo. Configura la BaseAddress real antes de usarlo.\");");

            return new GeneratedFile
            {
                RelativePath = GeneratedFilePathHelper.BuildRelativePath(ns.ClientNamespace, ns.ClientNamespace, "Program.cs"),
                Content = sb.ToString()
            };
        }
    }
}
