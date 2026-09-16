namespace ApiForge.Infrastructure.Generator.Planning
{
    public sealed record ClientGroupPlan
    {
        public required string GroupName { get; init; }
        public required string InterfaceName { get; init; }
        public required string ClassName { get; init; }
        public required IReadOnlyList<EndpointPlan> Endpoints { get; init; }
    }
}
