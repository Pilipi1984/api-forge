using ApiForge.Domain.GeneratedApiSolution;
using System.Text;

namespace ApiForge.Infrastructure.Generator
{
    /// <summary>
    /// Generates project and solution files for a given root namespace, including .csproj files for Domain, 
    /// Application, Infrastructure, and Client projects, as well as a .sln solution file.
    /// </summary>
    public static class ProjectFileGenerator
    {
        private const string TargetFramework = "net10.0";
        private const string EndProject = "EndProject";

        /// <summary>
        /// Generates the .csproj files for the Domain, Application, Infrastructure, and Client projects based on the provided root namespace.
        /// </summary>
        /// <param name="rootNamespace"></param>
        /// <returns>Returns the list of generated project files.</returns>
        public static List<GeneratedFile> GenerateProjectFiles(string rootNamespace)
        {
            var files = new List<GeneratedFile>
            {
                new()
                {
                    RelativePath = $"{rootNamespace}.Domain/{rootNamespace}.Domain.csproj",
                    Content = BuildCsproj(TargetFramework)
                },
                new()
                {
                    RelativePath = $"{rootNamespace}.Application/{rootNamespace}.Application.csproj",
                    Content = BuildCsproj(TargetFramework,
                        projectReferences: new[] { $@"..\{rootNamespace}.Domain\{rootNamespace}.Domain.csproj" })
                },
                new()
                {
                    RelativePath = $"{rootNamespace}.Infrastructure/{rootNamespace}.Infrastructure.csproj",
                    Content = BuildCsproj(TargetFramework,
                        projectReferences: new[]
                        {
                            $@"..\{rootNamespace}.Domain\{rootNamespace}.Domain.csproj",
                            $@"..\{rootNamespace}.Application\{rootNamespace}.Application.csproj"
                        },
                        packageReferences: new[]
                        {
                            ("Microsoft.Extensions.Http", "9.0.0"),
                            ("System.Net.Http.Json", "9.0.0")
                        })
                },
                new()
                {
                    RelativePath = $"{rootNamespace}.Client/{rootNamespace}.Client.csproj",
                    Content = BuildCsproj("net10.0",
                        projectReferences: new[]
                        {
                            $@"..\{rootNamespace}.Domain\{rootNamespace}.Domain.csproj",
                            $@"..\{rootNamespace}.Application\{rootNamespace}.Application.csproj",
                            $@"..\{rootNamespace}.Infrastructure\{rootNamespace}.Infrastructure.csproj"
                        },
                        packageReferences: new[] { ("Microsoft.Extensions.DependencyInjection", "9.0.0") },
                        outputType: "Exe")
                }
            };

            return files;
        }

        /// <summary>
        /// Generates a .sln solution file that includes the Domain, Application, Infrastructure, and Client projects based on the provided root namespace.
        /// </summary>
        /// <param name="rootNamespace"></param>
        /// <returns>Returns the generated solution file.</returns>
        public static GeneratedFile GenerateSolutionFile(string rootNamespace)
        {
            var domainGuid = Guid.NewGuid().ToString("B").ToUpperInvariant();
            var applicationGuid = Guid.NewGuid().ToString("B").ToUpperInvariant();
            var infrastructureGuid = Guid.NewGuid().ToString("B").ToUpperInvariant();
            var clientGuid = Guid.NewGuid().ToString("B").ToUpperInvariant();
            const string projectTypeGuid = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";

            var sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("Microsoft Visual Studio Solution File, Format Version 12.00");
            sb.AppendLine($"Project(\"{projectTypeGuid}\") = \"{rootNamespace}.Domain\", \"{rootNamespace}.Domain\\{rootNamespace}.Domain.csproj\", \"{domainGuid}\"");
            sb.AppendLine(EndProject);
            sb.AppendLine($"Project(\"{projectTypeGuid}\") = \"{rootNamespace}.Application\", \"{rootNamespace}.Application\\{rootNamespace}.Application.csproj\", \"{applicationGuid}\"");
            sb.AppendLine(EndProject);
            sb.AppendLine($"Project(\"{projectTypeGuid}\") = \"{rootNamespace}.Infrastructure\", \"{rootNamespace}.Infrastructure\\{rootNamespace}.Infrastructure.csproj\", \"{infrastructureGuid}\"");
            sb.AppendLine(EndProject);
            sb.AppendLine($"Project(\"{projectTypeGuid}\") = \"{rootNamespace}.Client\", \"{rootNamespace}.Client\\{rootNamespace}.Client.csproj\", \"{clientGuid}\"");
            sb.AppendLine(EndProject);
            sb.AppendLine("Global");
            sb.AppendLine("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution");
            sb.AppendLine("\t\tDebug|Any CPU = Debug|Any CPU");
            sb.AppendLine("\t\tRelease|Any CPU = Release|Any CPU");
            sb.AppendLine("\tEndGlobalSection");
            sb.AppendLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
            foreach (var guid in new[] { domainGuid, applicationGuid, infrastructureGuid, clientGuid })
            {
                sb.AppendLine($"\t\t{guid}.Debug|Any CPU.ActiveCfg = Debug|Any CPU");
                sb.AppendLine($"\t\t{guid}.Debug|Any CPU.Build.0 = Debug|Any CPU");
                sb.AppendLine($"\t\t{guid}.Release|Any CPU.ActiveCfg = Release|Any CPU");
                sb.AppendLine($"\t\t{guid}.Release|Any CPU.Build.0 = Release|Any CPU");
            }
            sb.AppendLine("\tEndGlobalSection");
            sb.AppendLine("\tGlobalSection(SolutionProperties) = preSolution");
            sb.AppendLine("\t\tHideSolutionNode = FALSE");
            sb.AppendLine("\tEndGlobalSection");
            sb.AppendLine("EndGlobal");

            return new GeneratedFile { RelativePath = $"{rootNamespace}.sln", Content = sb.ToString() };
        }

        /// <summary>
        /// Builds the content of a .csproj file based on the provided target framework, project references, package references, and output type.
        /// </summary>
        /// <param name="targetFramework"></param>
        /// <param name="projectReferences"></param>
        /// <param name="packageReferences"></param>
        /// <param name="outputType"></param>
        /// <returns>Returns the generated .csproj content.</returns>
        private static string BuildCsproj(
            string targetFramework,
            IEnumerable<string>? projectReferences = null,
            IEnumerable<(string Package, string Version)>? packageReferences = null,
            string? outputType = null)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<Project Sdk=\"Microsoft.NET.Sdk\">");
            sb.AppendLine();
            sb.AppendLine("  <PropertyGroup>");
            sb.AppendLine($"    <TargetFramework>{targetFramework}</TargetFramework>");
            sb.AppendLine("    <ImplicitUsings>enable</ImplicitUsings>");
            sb.AppendLine("    <Nullable>enable</Nullable>");
            if (outputType is not null)
                sb.AppendLine($"    <OutputType>{outputType}</OutputType>");
            sb.AppendLine("  </PropertyGroup>");

            if (packageReferences is not null && packageReferences.Any())
            {
                sb.AppendLine();
                sb.AppendLine("  <ItemGroup>");
                foreach (var (package, version) in packageReferences)
                    sb.AppendLine($"    <PackageReference Include=\"{package}\" Version=\"{version}\" />");
                sb.AppendLine("  </ItemGroup>");
            }

            if (projectReferences is not null && projectReferences.Any())
            {
                sb.AppendLine();
                sb.AppendLine("  <ItemGroup>");
                foreach (var reference in projectReferences)
                    sb.AppendLine($"    <ProjectReference Include=\"{reference}\" />");
                sb.AppendLine("  </ItemGroup>");
            }

            sb.AppendLine();
            sb.AppendLine("</Project>");
            return sb.ToString();
        }
    }
}
