using ApiForge.Domain.GeneratedApiSolution;
using ApiForge.Domain.Enums;

namespace ApiForge.Application.Interfaces
{
    /// <summary>
    /// Abstraction over the in-process generator used by the API layer.
    /// Allows the API project and tests to avoid a direct runtime dependency on the concrete generator assembly.
    /// </summary>
    public interface IApiForgeGenerator
    {
        Task<GeneratedSolution> GenerateAsync(Stream openApiSpec, ArchitectureStyle? architectureOverride = null);
        Task<GeneratedZipArchive> GenerateZipAsync(Stream openApiSpec, ArchitectureStyle? architectureOverride = null);
    }
}
