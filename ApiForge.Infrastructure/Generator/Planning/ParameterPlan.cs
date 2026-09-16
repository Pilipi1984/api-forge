using ApiForge.Domain.Models.ApiParameters;

namespace ApiForge.Infrastructure.Generator.Planning
{
    public sealed record ParameterPlan
    {
        public required string Name { get; init; }
        public required string CSharpType { get; init; }
        public required ApiParameter Source { get; init; }
    }
}
