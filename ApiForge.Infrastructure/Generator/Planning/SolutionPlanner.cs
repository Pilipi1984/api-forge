using ApiForge.Domain.Models;
using ApiForge.Domain.Models.ApiParameters;
using ApiForge.Infrastructure.Generator.Resolvers;
using ApiForge.Infrastructure.Helpers;

namespace ApiForge.Infrastructure.Generator.Planning
{
    public static class SolutionPlanner
    {
        public static SolutionPlan CreatePlan(ApiDefinition definition, string rootNamespace)
        {
            var modelsNamespace = $"{rootNamespace}.Domain.Models";
            var groups = new List<ClientGroupPlan>();

            foreach (var group in definition.Endpoints.GroupBy(ResolveGroupName))
            {
                var usedMethodNames = new HashSet<string>(StringComparer.Ordinal);
                var endpointPlans = new List<EndpointPlan>();

                foreach (var endpoint in group)
                {
                    var methodName = ResolveMethodName(endpoint, usedMethodNames);
                    var returnType = CSharpTypeResolver.Resolve(endpoint.Response, modelsNamespace);

                    var usedParamNames = new HashSet<string>(StringComparer.Ordinal) { "cancellationToken" };

                    var pathParams = BuildParameterPlans(endpoint.Parameters.OfType<ApiPathParameter>(), usedParamNames);
                    var queryParams = BuildParameterPlans(endpoint.Parameters.OfType<ApiQueryParameter>(), usedParamNames);
                    var headerParams = BuildParameterPlans(endpoint.Parameters.OfType<ApiHeaderParameter>(), usedParamNames);

                    string? requestBodyType = null;
                    string? requestBodyParamName = null;
                    if (endpoint.RequestBody is not null)
                    {
                        requestBodyType = CSharpTypeResolver.Resolve(endpoint.RequestBody, modelsNamespace);
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

            return new SolutionPlan { RootNamespace = rootNamespace, Groups = groups };
        }

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

        private static string MakeUnique(string candidate, HashSet<string> used)
        {
            var name = candidate;
            var i = 1;
            while (!used.Add(name))
                name = candidate + i++;
            return name;
        }

        private static string ResolveGroupName(ApiEndpoint endpoint)
        {
            var firstSegment = endpoint.Route
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .FirstOrDefault(s => !s.StartsWith('{'));

            return firstSegment ?? "Root";
        }

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
