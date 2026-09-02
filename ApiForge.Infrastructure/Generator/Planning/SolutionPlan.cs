using ApiForge.Domain.Models.ApiParameters;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiForge.Infrastructure.Generator.Planning
{
    public sealed record ParameterPlan
    {
        public required string Name { get; init; }
        public required string CSharpType { get; init; }
        public required ApiParameter Source { get; init; }
    }

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

    public sealed record ClientGroupPlan
    {
        public required string GroupName { get; init; }
        public required string InterfaceName { get; init; }
        public required string ClassName { get; init; }
        public required IReadOnlyList<EndpointPlan> Endpoints { get; init; }
    }

    public sealed record SolutionPlan
    {
        public required string RootNamespace { get; init; }
        public required IReadOnlyList<ClientGroupPlan> Groups { get; init; }
    }
}
