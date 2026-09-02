using ApiForge.Domain.GeneratedApiSolution;
using ApiForge.Infrastructure.Generator.Planning;
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
        /// <param name="rootNamespace"></param>
        /// <returns>Returns the generated Program.cs file.</returns>
        public static GeneratedFile Generate(SolutionPlan plan, string rootNamespace)
        {
            var sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
            sb.AppendLine($"using {rootNamespace}.Infrastructure.DependencyInjection;");
            sb.AppendLine();
            sb.AppendLine("var services = new ServiceCollection();");
            sb.AppendLine($"services.Add{rootNamespace}Clients(new Uri(\"https://localhost\"));");
            sb.AppendLine();
            sb.AppendLine("using var provider = services.BuildServiceProvider();");
            sb.AppendLine();
            sb.AppendLine("Console.WriteLine(\"Cliente de API generado y listo. Configura la BaseAddress real antes de usarlo.\");");

            return new GeneratedFile
            {
                RelativePath = $"{rootNamespace}.Client/Program.cs",
                Content = sb.ToString()
            };
        }
    }
}
