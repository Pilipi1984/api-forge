namespace ApiForge.Infrastructure.Generator.Planning
{
    /// <summary>
    /// Planned endpoint
    /// </summary>
    public sealed record EndpointPlan
    {
        /// <summary>
        /// Name of method
        /// </summary>
        public required string MethodName { get; init; }
        /// <summary>
        /// Http method
        /// </summary>
        public required string HttpMethod { get; init; }
        /// <summary>
        /// Endpoint route
        /// </summary>
        public required string Route { get; init; }
        /// <summary>
        /// Type of return value
        /// </summary>
        public required string ReturnType { get; init; }
        /// <summary>
        /// List of path parameters
        /// </summary>
        public required IReadOnlyList<ParameterPlan> PathParameters { get; init; }
        /// <summary>
        /// List of query parameters
        /// </summary>
        public required IReadOnlyList<ParameterPlan> QueryParameters { get; init; }
        /// <summary>
        /// List of header parameters
        /// </summary>
        public required IReadOnlyList<ParameterPlan> HeaderParameters { get; init; }
        /// <summary>
        /// Type of requested body
        /// </summary>
        public string? RequestBodyType { get; init; }
        /// <summary>
        /// requested body parameter name
        /// </summary>
        public string? RequestBodyParameterName { get; init; }
        /// <summary>
        /// The summary
        /// </summary>
        public string? Summary { get; init; }
    }
}
