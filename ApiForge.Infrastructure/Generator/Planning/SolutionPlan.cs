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
        /// <summary>
        /// Root namespace.
        /// </summary>
        public required string RootNamespace { get; init; }
        /// <summary>
        /// Style of architecture.
        /// </summary>
        public required ArchitectureStyle Style { get; init; }
        /// <summary>
        /// Architecture conventions
        /// </summary>
        public required ArchitectureConventions Conventions { get; init; }
        /// <summary>
        /// Project namespaces
        /// </summary>
        public required ProjectNamespaces Namespaces { get; init; }
        /// <summary>
        /// List of client group plans
        /// </summary>
        public required IReadOnlyList<ClientGroupPlan> Groups { get; init; }
    }
}
