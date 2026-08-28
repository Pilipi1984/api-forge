using ApiForge.Domain.GeneratedApiSolution;
using ApiForge.Domain.Models;

namespace ApiForge.Application.Interfaces
{
    public interface ICodeGenerator
    {
        Task<GeneratedSolution> GenerateAsync(ApiDefinition definition);
    }
}
