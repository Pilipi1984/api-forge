using ApiForge.Domain.Models;
using ApiForge.Domain.Models.ApiParameters;
using ApiForge.Infrastructure.Generator.Resolvers;
using ApiForge.Infrastructure.Helpers;

namespace ApiForge.Infrastructure.Generator.Planning
{
    /// <summary>
    /// Generates a plan for creating a C# solution based on the provided API definition, 
    /// organizing endpoints into client groups and resolving method names, parameter types, and namespaces.
    /// </summary>
    public static class SolutionPlanner
    {
        /// <summary>
        /// Creates a solution plan based on the provided API definition and root namespace.
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="rootNamespace"></param>
        /// <returns>The generated solution plan.</returns>
        public static SolutionPlan CreatePlan(ApiDefinition definition, string rootNamespace)
        {
            var namespaces = ProjectNamespaces.From(rootNamespace);
            var groups = new List<ClientGroupPlan>();

            foreach (var group in definition.Endpoints.GroupBy(ResolveGroupName))
            {
                var usedMethodNames = new HashSet<string>(StringComparer.Ordinal);
                var endpointPlans = new List<EndpointPlan>();

                foreach (var endpoint in group)
                {
                    var methodName = ResolveMethodName(endpoint, usedMethodNames);
                    var returnType = CSharpTypeResolver.Resolve(endpoint.Response, namespaces.DomainModelsNamespace);

                    var usedParamNames = new HashSet<string>(StringComparer.Ordinal) { "cancellationToken" };

                    var pathParams = BuildParameterPlans(endpoint.Parameters.OfType<ApiPathParameter>(), usedParamNames);
                    var queryParams = BuildParameterPlans(endpoint.Parameters.OfType<ApiQueryParameter>(), usedParamNames);
                    var headerParams = BuildParameterPlans(endpoint.Parameters.OfType<ApiHeaderParameter>(), usedParamNames);

                    string? requestBodyType = null;
                    string? requestBodyParamName = null;
                    if (endpoint.RequestBody is not null)
                    {
                        requestBodyType = CSharpTypeResolver.Resolve(endpoint.RequestBody, namespaces.DomainModelsNamespace);
                        requestBodyParamName = MakeUnique("request", usedParamNames);
                    }

                    endpointPlans.Add(new EndpointPlan
                    {
                        MethodName = methodName,
                        HttpMethod = endpoint.HttpMethod,
                        Route = endpoint.Route,
                        ReturnType = string.IsNullOrWhiteSpace(returnType) ? "object" : returnType,
                        PathParameters = pathParams,
                        QueryParameters = queryParams,
                        HeaderParameters = headerParams,
                        RequestBodyType = requestBodyType,
                        RequestBodyParameterName = requestBodyParamName,
                        Summary = endpoint.Summary
                    });
                }

                var pascalGroup = NameHelper.ToPascalCase(group.Key);
                groups.Add(new ClientGroupPlan
                {
                    GroupName = pascalGroup,
                    InterfaceName = $"I{pascalGroup}Client",
                    ClassName = $"{pascalGroup}Client",
                    Endpoints = endpointPlans
                });
            }

            return new SolutionPlan { RootNamespace = rootNamespace, Namespaces = namespaces, Groups = groups };
        }

        /// <summary>
        /// Builds a list of parameter plans from the provided API parameters, ensuring unique names and resolving C# types.
        /// </summary>
        /// <param name="parameters"></param>
        /// <param name="usedNames"></param>
        /// <returns>The list of generated parameter plans.</returns>
        private static List<ParameterPlan> BuildParameterPlans(IEnumerable<ApiParameter> parameters, HashSet<string> usedNames)
        {
            var result = new List<ParameterPlan>();
            foreach (var parameter in parameters)
            {
                var name = MakeUnique(NameHelper.ToValidIdentifier(parameter.Name), usedNames);
                var type = CSharpTypeResolver.NormalizePrimitive(parameter.Type);
                if (!parameter.Required)
                    type += "?";

                result.Add(new ParameterPlan { Name = name, CSharpType = type, Source = parameter });
            }
            return result;
        }

        /// <summary>
        /// Ensures that the provided candidate name is unique within the given set of used names.
        /// </summary>
        /// <param name="candidate"></param>
        /// <param name="used"></param>
        /// <returns>The unique name.</returns>
        private static string MakeUnique(string candidate, HashSet<string> used)
        {
            var name = candidate;
            var i = 1;
            while (!used.Add(name))
                name = candidate + i++;
            return name;
        }

        /// <summary>
        /// Resolves the group name for the provided API endpoint based on its route. 
        /// The first non-parameter segment of the route is used as the group name. If no such segment exists, "Root" is returned.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns>The resolved group name.</returns>
        private static string ResolveGroupName(ApiEndpoint endpoint)
        {
            var firstSegment = endpoint.Route
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault(s => !s.StartsWith('{'));

            return firstSegment ?? "Root";
        }

        /// <summary>
        /// Resolves a unique method name for the provided API endpoint. If the endpoint has an OperationId, it is used as the base name; 
        /// otherwise, the HTTP method and route are combined to form the base name. The method ensures that the final name is unique within the provided set of used names.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <param name="used"></param>
        /// <returns>The resolved method name.</returns>
        private static string ResolveMethodName(ApiEndpoint endpoint, HashSet<string> used)
        {
            string baseName;
            if (!string.IsNullOrWhiteSpace(endpoint.OperationId))
            {
                baseName = NameHelper.ToPascalCase(endpoint.OperationId);
            }
            else
            {
                var routePart = string.Concat(endpoint.Route
                    .Split('/', StringSplitOptions.RemoveEmptyEntries)
                    .Select(NameHelper.ToPascalCase));
                baseName = $"{NameHelper.ToPascalCase(endpoint.HttpMethod)}{routePart}";
            }

            return MakeUnique(baseName, used);
        }
    }
}
