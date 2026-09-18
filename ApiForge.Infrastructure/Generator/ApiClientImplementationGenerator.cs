using ApiForge.Domain.GeneratedApiSolution;
using ApiForge.Infrastructure.Generator.Planning;
using ApiForge.Infrastructure.Helpers;
using System.Text;

namespace ApiForge.Infrastructure.Generator
{
    /// <summary>
    /// Generates C# implementation files for API clients based on the provided solution plan, 
    /// creating a class for each group of endpoints that implements the corresponding interface and handles HTTP requests and responses.
    /// </summary>
    public static class ApiClientImplementationGenerator
    {
        /// <summary>
        /// Generates C# implementation files for API clients based on the provided solution plan.
        /// </summary>
        /// <param name="plan"></param>
        /// <returns>Returns a list of generated files.</returns>
        public static List<GeneratedFile> Generate(SolutionPlan plan)
        {
            var files = new List<GeneratedFile>();
            var ns = plan.Namespaces;

            foreach (var group in plan.Groups)
            {
                var sb = new StringBuilder();
                sb.AppendLine("using System;");
                sb.AppendLine("using System.Collections.Generic;");
                sb.AppendLine("using System.Net.Http;");
                sb.AppendLine("using System.Net.Http.Json;");
                sb.AppendLine("using System.Threading;");
                sb.AppendLine("using System.Threading.Tasks;");
                sb.AppendLine($"using {ns.DomainModelsNamespace};");
                sb.AppendLine($"using {ns.ApplicationInterfacesNamespace};");
                sb.AppendLine();
                sb.AppendLine($"namespace {ns.InfrastructureClientsNamespace}");
                sb.AppendLine("{");
                sb.AppendLine($"    public sealed class {group.ClassName} : {group.InterfaceName}");
                sb.AppendLine("    {");
                sb.AppendLine("        private readonly HttpClient _httpClient;");
                sb.AppendLine();
                sb.AppendLine($"        public {group.ClassName}(HttpClient httpClient)");
                sb.AppendLine("        {");
                sb.AppendLine("            _httpClient = httpClient;");
                sb.AppendLine("        }");

                foreach (var endpoint in group.Endpoints)
                {
                    sb.AppendLine();
                    AppendMethod(sb, endpoint);
                }

                sb.AppendLine("    }");
                sb.AppendLine("}");

                files.Add(new GeneratedFile
                {
                    RelativePath = GeneratedFilePathHelper.BuildRelativePath(
                        ns.InfrastructureNamespace, ns.InfrastructureClientsNamespace, $"{group.ClassName}.cs"),
                    Content = sb.ToString()
                });
            }

            return files;
        }

        /// <summary>
        /// Appends a method implementation for the given endpoint to the provided StringBuilder, generating the necessary HTTP request and response handling code.
        /// </summary>
        /// <param name="sb"></param>
        /// <param name="endpoint"></param>
        private static void AppendMethod(StringBuilder sb, EndpointPlan endpoint)
        {
            ArgumentNullException.ThrowIfNull(endpoint);
            var signatureParams = EndpointSignatureHelper.BuildParameterList(endpoint);
            sb.AppendLine($"\t\tpublic async Task<{endpoint.ReturnType}> {endpoint.MethodName}Async({signatureParams}CancellationToken cancellationToken = default)");
            sb.AppendLine("\t\t{");
            sb.AppendLine($"\t\t\tvar path = {BuildPathExpression(endpoint)};");

            if (endpoint.QueryParameters.Count > 0)
            {
                sb.AppendLine("\t\t\tvar queryParameters = new List<string>();");
                foreach (var query in endpoint.QueryParameters)
                {
                    sb.AppendLine($"\t\t\tif ({query.Name} is not null)");
                    sb.AppendLine("\t\t\t{");
                    sb.AppendLine($"\t\t\t\tqueryParameters.Add($\"{query.Source.Name}={{Uri.EscapeDataString({query.Name}.ToString() ?? string.Empty)}}\");");
                    sb.AppendLine("\t\t\t}");
                    sb.AppendLine();
                }
                sb.AppendLine("\t\t\tif (queryParameters.Count > 0)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine("\t\t\t\tpath += \"?\" + string.Join(\"&\", queryParameters);");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();
            }

            sb.AppendLine($"\t\t\tusing var httpRequest = new HttpRequestMessage({ToHttpMethodExpression(endpoint.HttpMethod)}, path);");

            foreach (var header in endpoint.HeaderParameters)
            {
                sb.AppendLine($"\t\t\tif ({header.Name} is not null)");
                sb.AppendLine("\t\t\t{");
                sb.AppendLine($"\t\t\t\thttpRequest.Headers.TryAddWithoutValidation(\"{header.Source.Name}\", {header.Name}.ToString());");
                sb.AppendLine("\t\t\t}");
                sb.AppendLine();
            }

            if (endpoint.RequestBodyType is not null)
            {
                sb.AppendLine($"\t\t\thttpRequest.Content = JsonContent.Create({endpoint.RequestBodyParameterName});");
            }

            sb.AppendLine("\t\t\tusing var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);");
            sb.AppendLine("\t\t\tresponse.EnsureSuccessStatusCode();");
            sb.AppendLine();

            if (!endpoint.ReturnType.Equals("object", StringComparison.InvariantCultureIgnoreCase))
            {
                sb.AppendLine($"\t\t\tvar result = await response.Content.ReadFromJsonAsync<{endpoint.ReturnType}>(cancellationToken: cancellationToken).ConfigureAwait(false);");
                sb.AppendLine();
            }

            sb.AppendLine("\t\t\treturn result!;");

            sb.AppendLine("\t\t}");
        }

        /// <summary>
        /// Builds a path expression for the given endpoint by replacing path parameter 
        /// placeholders with their corresponding names, returning a string that can be used in the generated method implementation.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns>Returns the generated path expression.</returns>
        private static string BuildPathExpression(EndpointPlan endpoint)
        {
            var template = endpoint.Route.TrimStart('/');
            foreach (var pathParam in endpoint.PathParameters)
            {
                template = template.Replace("{" + pathParam.Source.Name + "}", "{" + pathParam.Name + "}", StringComparison.Ordinal);
            }

            return $"$\"{template}\"";
        }

        /// <summary>
        /// Converts an HTTP method string to its corresponding HttpMethod expression in C#, returning a string that can be used in the generated method implementation.
        /// </summary>
        /// <param name="httpMethod"></param>
        /// <returns>Returns the generated HttpMethod expression.</returns>
        private static string ToHttpMethodExpression(string httpMethod) => httpMethod.ToUpperInvariant() switch
        {
            "GET" => "HttpMethod.Get",
            "POST" => "HttpMethod.Post",
            "PUT" => "HttpMethod.Put",
            "DELETE" => "HttpMethod.Delete",
            "PATCH" => "HttpMethod.Patch",
            "HEAD" => "HttpMethod.Head",
            "OPTIONS" => "HttpMethod.Options",
            "TRACE" => "HttpMethod.Trace",
            _ => "HttpMethod.Get"
        };
    }
}
