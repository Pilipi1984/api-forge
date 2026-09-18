using ApiForge.Domain.GeneratedApiSolution;
using ApiForge.Domain.Models;

namespace ApiForge.Application.Interfaces
{
    /// <summary>
    /// Interface for Code Generator
    /// </summary>
    public interface ICodeGenerator
    {
        /// <summary>
        /// Generates the solution.
        /// </summary>
        /// <param name="definition">ApiDefinition</param>
        /// <returns>The generated solution from the definition</returns>
        Task<GeneratedSolution> GenerateAsync(ApiDefinition definition);
    }
}
