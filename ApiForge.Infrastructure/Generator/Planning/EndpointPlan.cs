namespace ApiForge.Infrastructure.Generator.Planning
{
    public sealed record EndpointPlan
    {
        public required string MethodName { get; init; }
        public required string HttpMethod { get; init; }
        public required string Route { get; init; }
        public required string ReturnType { get; init; }
        public required IReadOnlyList<ParameterPlan> PathParameters { get; init; }
        public required IReadOnlyList<ParameterPlan> QueryParameters { get; init; }
        public required IReadOnlyList<ParameterPlan> HeaderParameters { get; init; }
        public string? RequestBodyType { get; init; }
        public string? RequestBodyParameterName { get; init; }
        public string? Summary { get; init; }
    }
}
