using ApiForge.Infrastructure.Generator.Planning;

namespace ApiForge.Infrastructure.Helpers
{
    internal static class EndpointSignatureHelper
    {
        public static string BuildParameterList(EndpointPlan endpoint)
        {
            var all = new List<string>();
            all.AddRange(endpoint.PathParameters.Select(p => $"{p.CSharpType} {p.Name}"));
            all.AddRange(endpoint.QueryParameters.Select(p => $"{p.CSharpType} {p.Name}"));
            all.AddRange(endpoint.HeaderParameters.Select(p => $"{p.CSharpType} {p.Name}"));
            if (endpoint.RequestBodyType is not null)
                all.Add($"{endpoint.RequestBodyType} {endpoint.RequestBodyParameterName}");

            return all.Count == 0 ? string.Empty : string.Join(", ", all) + ", ";
        }
    }
}