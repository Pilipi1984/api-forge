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
        /// <summary>
        /// Generates a solution from an OpenAPI specification stream.
        /// </summary>
        /// <param name="openApiSpec">OpenAPI specification stream.</param>
        /// <param name="architectureOverride">Optional architecture override.</param>
        /// <returns>Generated solution.</returns>
        Task<GeneratedSolution> GenerateAsync(Stream openApiSpec, ArchitectureStyle? architectureOverride = null);
        /// <summary>
        /// Generates a zip archive from an OpenAPI specification stream.
        /// </summary>
        /// <param name="openApiSpec">OpenAPI specification stream.</param>
        /// <param name="architectureOverride">Optional architecture override.</param>
        /// <returns>Generated zip archive.</returns>
        Task<GeneratedZipArchive> GenerateZipAsync(Stream openApiSpec, ArchitectureStyle? architectureOverride = null);
    }
}
