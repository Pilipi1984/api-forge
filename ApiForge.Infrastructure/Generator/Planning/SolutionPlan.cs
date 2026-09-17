using ApiForge.Domain.Enums;

namespace ApiForge.Infrastructure.Generator.Planning
{
    /// <summary>
    /// The resolved plan for generating a solution: root namespace, detected/selected architecture
    /// style, the naming conventions derived from that style, the concrete per-layer namespaces
    /// (<see cref="Namespaces"/>), and the endpoint groups to generate clients/adapters for.
    /// </summary>
    public sealed record SolutionPlan
    {
        public required string RootNamespace { get; init; }
        public required ArchitectureStyle Style { get; init; }
        public required ArchitectureConventions Conventions { get; init; }
        public required ProjectNamespaces Namespaces { get; init; }
        public required IReadOnlyList<ClientGroupPlan> Groups { get; init; }
    }
}
