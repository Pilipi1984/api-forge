using ApiForge.Domain.Models;

namespace ApiForge.Application.Interfaces
{
    public interface IOpenApiParser
    {
        Task<ApiDefinition> ParseAsync(Stream stream);
    }
}
