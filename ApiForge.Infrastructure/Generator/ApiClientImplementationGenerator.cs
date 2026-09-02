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
            var modelsNamespace = $"{plan.RootNamespace}.Domain.Models";
            var interfacesNamespace = $"{plan.RootNamespace}.Application.Interfaces";
            var clientsNamespace = $"{plan.RootNamespace}.Infrastructure.Clients";

            foreach (var group in plan.Groups)
            {
                var sb = new StringBuilder();
                sb.AppendLine("using System;");
                sb.AppendLine("using System.Collections.Generic;");
                sb.AppendLine("using System.Net.Http;");
                sb.AppendLine("using System.Net.Http.Json;");
                sb.AppendLine("using System.Threading;");
                sb.AppendLine("using System.Threading.Tasks;");
                sb.AppendLine($"using {modelsNamespace};");
                sb.AppendLine($"using {interfacesNamespace};");
                sb.AppendLine();
                sb.AppendLine($"namespace {clientsNamespace}");
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
                    RelativePath = $"{plan.RootNamespace}.Infrastructure/Clients/{group.ClassName}.cs",
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
        private static void AppendMethod(StringBuilder sb, Planning.EndpointPlan endpoint)
        {
            var signatureParams = EndpointSignatureHelper.BuildParameterList(endpoint);
            sb.AppendLine($"        public async Task<{endpoint.ReturnType}> {endpoint.MethodName}Async({signatureParams}CancellationToken cancellationToken = default)");
            sb.AppendLine("        {");
            sb.AppendLine($"            var path = {BuildPathExpression(endpoint)};");

            if (endpoint.QueryParameters.Count > 0)
            {
                sb.AppendLine("            var queryParameters = new List<string>();");
                foreach (var query in endpoint.QueryParameters)
                {
                    sb.AppendLine($"            if ({query.Name} is not null)");
                    sb.AppendLine($"                queryParameters.Add($\"{query.Source.Name}={{Uri.EscapeDataString({query.Name}.ToString() ?? string.Empty)}}\");");
                }
                sb.AppendLine("            if (queryParameters.Count > 0)");
                sb.AppendLine("                path += \"?\" + string.Join(\"&\", queryParameters);");
            }

            sb.AppendLine($"            using var httpRequest = new HttpRequestMessage({ToHttpMethodExpression(endpoint.HttpMethod)}, path);");

            foreach (var header in endpoint.HeaderParameters)
            {
                sb.AppendLine($"            if ({header.Name} is not null)");
                sb.AppendLine($"                httpRequest.Headers.TryAddWithoutValidation(\"{header.Source.Name}\", {header.Name}.ToString());");
            }

            if (endpoint.RequestBodyType is not null)
                sb.AppendLine($"            httpRequest.Content = JsonContent.Create({endpoint.RequestBodyParameterName});");

            sb.AppendLine("            using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);");
            sb.AppendLine("            response.EnsureSuccessStatusCode();");
            sb.AppendLine();

            if (endpoint.ReturnType == "object")
            {
                sb.AppendLine("            return default!;");
            }
            else
            {
                sb.AppendLine($"            var result = await response.Content.ReadFromJsonAsync<{endpoint.ReturnType}>(cancellationToken: cancellationToken).ConfigureAwait(false);");
                sb.AppendLine("            return result!;");
            }

            sb.AppendLine("        }");
        }

        /// <summary>
        /// Builds a path expression for the given endpoint by replacing path parameter 
        /// placeholders with their corresponding names, returning a string that can be used in the generated method implementation.
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns>Returns the generated path expression.</returns>
        private static string BuildPathExpression(Planning.EndpointPlan endpoint)
        {
            var template = endpoint.Route.TrimStart('/');
            foreach (var pathParam in endpoint.PathParameters)
                template = template.Replace("{" + pathParam.Source.Name + "}", "{" + pathParam.Name + "}", StringComparison.Ordinal);

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
