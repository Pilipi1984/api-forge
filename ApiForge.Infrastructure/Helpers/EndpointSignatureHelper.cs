using ApiForge.Infrastructure.Generator.Planning;

namespace ApiForge.Infrastructure.Helpers
{
    /// <summary>
    /// Provides helper methods for building endpoint signatures, particularly for generating parameter lists from endpoint plans.
    /// </summary>
    internal static class EndpointSignatureHelper
    {
        /// <summary>
        /// Builds a parameter list string for a given endpoint plan, including path, query, header parameters, and request body if present.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns>Returns the parameter list string.</returns>
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