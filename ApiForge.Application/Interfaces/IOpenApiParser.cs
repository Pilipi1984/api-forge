using ApiForge.Domain.Models;

namespace ApiForge.Application.Interfaces
{
    /// <summary>
    /// Interface for OpenApiParser
    /// </summary>
    public interface IOpenApiParser
    {
        /// <summary>
        /// Parses from stream to ApiDefinition
        /// </summary>
        /// <param name="stream">The stream of the source file</param>
        /// <returns>ApiDefinition</returns>
        Task<ApiDefinition> ParseAsync(Stream stream);
    }
}
