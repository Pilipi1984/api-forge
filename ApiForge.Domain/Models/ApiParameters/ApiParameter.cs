using Microsoft.OpenApi;

namespace ApiForge.Domain.Models.ApiParameters
{
    public abstract class ApiParameter
    {
        public required string Name { get; init; }

        public required string Type { get; init; }

        public required ParameterLocation Location { get; init; }

        public bool Required { get; init; }

        public string? Description { get; init; }
    }
}
