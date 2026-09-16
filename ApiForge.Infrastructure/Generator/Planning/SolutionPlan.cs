using ApiForge.Domain.Enums;

namespace ApiForge.Infrastructure.Generator.Planning
{

    public sealed record SolutionPlan
    {
        public required string RootNamespace { get; init; }
        public required ArchitectureStyle Style { get; init; }
        public required ArchitectureConventions Conventions { get; init; }
        public required IReadOnlyList<ClientGroupPlan> Groups { get; init; }
    }
}
