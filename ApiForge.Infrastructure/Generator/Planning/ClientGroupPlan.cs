namespace ApiForge.Infrastructure.Generator.Planning
{
    /// <summary>
    /// Represents the generation plan of a client for a group of endpoints, including the group name, interface name, class name, and the collection of endpoints.
    /// </summary>
    public sealed record ClientGroupPlan
    {
        /// <summary>
        /// Name of group
        /// </summary>
        public required string GroupName { get; init; }
        /// <summary>
        /// Name of interface
        /// </summary>
        public required string InterfaceName { get; init; }
        /// <summary>
        /// Name of class
        /// </summary>
        public required string ClassName { get; init; }
        /// <summary>
        /// The list of endpoints
        /// </summary>
        public required IReadOnlyList<EndpointPlan> Endpoints { get; init; }
    }
}
