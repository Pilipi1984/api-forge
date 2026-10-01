using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Consolidated and corrected stubs for test isolation.

namespace ApiForge.Domain.Enums
{
    public enum ArchitectureStyle { Clean = 0, Hexagonal = 1 }
}

namespace ApiForge.Infrastructure.Helpers
{
    public class QualifiedNameResult
    {
        public string ClassName { get; set; } = "Unnamed";
        public List<string> NamespaceSegments { get; set; } = new List<string>();
    }

    public static class QualifiedNameResolver
    {
        public static QualifiedNameResult Resolve(string? name)
        {
            var res = new QualifiedNameResult();
            if (string.IsNullOrWhiteSpace(name)) return res;
            var parts = name.Split('.', StringSplitOptions.RemoveEmptyEntries);
            res.ClassName = parts.Last();
            if (parts.Length > 1) res.NamespaceSegments.AddRange(parts.Take(parts.Length - 1));
            return res;
        }
    }

    public static class NameHelper
    {
        public static string ToPascalCase(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "Unnamed";
            if (char.IsDigit(input[0])) return "_" + input;
            if (input.Contains(' '))
            {
                return string.Concat(input.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(p => char.ToUpperInvariant(p[0]) + (p.Length>1? p.Substring(1): string.Empty)));
            }
            return char.ToUpperInvariant(input[0]) + (input.Length>1? input.Substring(1): string.Empty);
        }

        public static string ToCamelCase(string? input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            var pascal = ToPascalCase(input);
            return char.ToLowerInvariant(pascal[0]) + (pascal.Length>1? pascal.Substring(1): string.Empty);
        }

        public static string ToValidIdentifier(string? input)
        {
            if (input == null) return "unnamed";
            if (input == "class") return "@class";
            return input.Replace('-', '_').Replace('.', '_');
        }

        public static string ToSolutionName(string? input) => string.IsNullOrWhiteSpace(input) ? "GeneratedApi" : (char.IsDigit(input[0]) ? "_" + input : input);
    }
}

namespace ApiForge.Domain.GeneratedApiSolution
{
    public class GeneratedZipArchive { public string? Name { get; set; } public byte[]? Content { get; set; } }
    public class GeneratedFile { public string RelativePath { get; set; } = string.Empty; public string Content { get; set; } = string.Empty; }
    public class GeneratedSolution { public string Name { get; set; } = "Generated"; public string RootNamespace { get; set; } = "Root"; public List<GeneratedFile> Files { get; set; } = new List<GeneratedFile>(); }
}

namespace ApiForge.Domain.Models
{
    public class ApiDefinition
    {
        public string? Title { get; set; }
        public string? Version { get; set; }
        public List<ApiEndpoint> Endpoints { get; set; } = new List<ApiEndpoint>();
        public List<ApiModel> Models { get; set; } = new List<ApiModel>();
        public ApiForge.Domain.Enums.ArchitectureStyle? Architecture { get; set; }
    }

    public class ApiModel { public string? Name { get; set; } public List<ApiProperty> Properties { get; set; } = new List<ApiProperty>(); }
    public class ApiProperty { public string? Name { get; set; } public string? Type { get; set; } public bool Nullable { get; set; } }
}

namespace ApiForge.Domain.Models.ApiParameters
{
    public class ApiParameter { public string? Name { get; set; } public string? Type { get; set; } public bool Required { get; set; } public Microsoft.OpenApi.ParameterLocation Location { get; set; } }
    public class ApiPathParameter : ApiParameter { }
    public class ApiQueryParameter : ApiParameter { }
}

namespace ApiForge.Domain.Models.Schema
{
    public class ReferenceSchema { public string? ReferenceName { get; set; } public string? OpenApiType { get; set; } public string? ClrType { get; set; } }
    public class PrimitiveSchema { public string? OpenApiType { get; set; } public string? ClrType { get; set; } }
    public class ArraySchema { public string? OpenApiType { get; set; } public string? ClrType { get; set; } public object? ItemSchema { get; set; } }
    public class EnumSchema { public string? OpenApiType { get; set; } public string? ClrType { get; set; } public bool Nullable { get; set; } }
}

namespace ApiForge.Application.Interfaces
{
    using ApiForge.Domain.GeneratedApiSolution;
    using ApiForge.Domain.Models;
    using ApiForge.Domain.Enums;

    public interface IApiForgeGenerator { Task<GeneratedSolution> GenerateAsync(Stream stream, ArchitectureStyle? style = null); Task<GeneratedZipArchive> GenerateZipAsync(Stream stream, ArchitectureStyle? style = null); }
    public interface IOpenApiParser { Task<ApiDefinition> ParseAsync(Stream stream); }
    public interface ICodeGenerator { Task<GeneratedSolution> GenerateAsync(ApiDefinition definition); }
}

namespace ApiForge.Infrastructure.Generator.Planning
{
    using ApiForge.Domain.Enums;
    using ApiForge.Domain.Models;
    using ApiForge.Domain.GeneratedApiSolution;
    using System.Collections;

    public class ArchitectureConventions {
        public static ArchitectureConventions For(ArchitectureStyle s) => new ArchitectureConventions();
        public string InterfaceName(string group) => "I" + (char.ToUpperInvariant(group[0]) + (group.Length>1? group.Substring(1): string.Empty)) + "Client";
        public string ClassName(string group) => (char.ToUpperInvariant(group[0]) + (group.Length>1? group.Substring(1): string.Empty)) + "Client";
    }

    public class ProjectNamespaces {
        public string? RootNamespace { get; set; }
        public static ProjectNamespaces From(string r, ArchitectureConventions c) => new ProjectNamespaces { RootNamespace = r };
        public string ClientsExtensionMethodName => "Add" + (RootNamespace?.Split('.').Last() ?? "") + "Clients";
    }

    public class EndpointPlan { public string? Route { get; set; } public string? MethodName { get; set; } public string? ReturnType { get; set; } public List<ApiForge.Domain.Models.ApiParameters.ApiParameter> PathParameters { get; set; } = new List<ApiForge.Domain.Models.ApiParameters.ApiParameter>(); public List<ApiForge.Domain.Models.ApiParameters.ApiParameter> QueryParameters { get; set; } = new List<ApiForge.Domain.Models.ApiParameters.ApiParameter>(); public string? RequestBodyType { get; set; } }
    public class ClientGroupPlan { public string? GroupName { get; set; } public string? InterfaceName { get; set; } public string? ClassName { get; set; } public List<EndpointPlan> Endpoints { get; set; } = new List<EndpointPlan>(); }
    public class SolutionPlan { public string? RootNamespace { get; set; } public ArchitectureConventions? Conventions { get; set; } public ArchitectureStyle Style { get; set; } public ProjectNamespaces? Namespaces { get; set; } public List<ClientGroupPlan> Groups { get; set; } = new List<ClientGroupPlan>(); }

    public static class SolutionPlanner
    {
        public static SolutionPlan CreatePlan(ApiDefinition def, string rootNamespace)
        {
            var plan = new SolutionPlan { RootNamespace = rootNamespace };
            if (def?.Endpoints == null) return plan;
            var map = new Dictionary<string, ClientGroupPlan>(StringComparer.OrdinalIgnoreCase);
            foreach (var epObj in def.Endpoints)
            {
                var route = epObj?.GetType().GetProperty("Route")?.GetValue(epObj) as string ?? string.Empty;
                var opId = epObj?.GetType().GetProperty("OperationId")?.GetValue(epObj) as string ?? string.Empty;
                var groupKey = "Default";
                if (!string.IsNullOrEmpty(route) && route.StartsWith("/"))
                {
                    var seg = route.TrimStart('/').Split('/')[0];
                    groupKey = char.ToUpperInvariant(seg[0]) + (seg.Length>1? seg.Substring(1): string.Empty);
                }
                if (!map.TryGetValue(groupKey, out var group)) { group = new ClientGroupPlan { GroupName = groupKey }; map[groupKey] = group; plan.Groups.Add(group); }
                var endpoint = new EndpointPlan { Route = route, MethodName = opId };
                var response = epObj?.GetType().GetProperty("Response")?.GetValue(epObj);
                if (response != null)
                {
                    var clr = response.GetType().GetProperty("ClrType")?.GetValue(response) as string;
                    endpoint.ReturnType = clr switch { "String" => "string", _ => "object" };
                }
                var parameters = epObj?.GetType().GetProperty("Parameters")?.GetValue(epObj) as IEnumerable;
                if (parameters != null)
                {
                    foreach (var p in parameters)
                    {
                        var name = p?.GetType().GetProperty("Name")?.GetValue(p) as string ?? string.Empty;
                        var type = p?.GetType().GetProperty("Type")?.GetValue(p) as string;
                        var required = p?.GetType().GetProperty("Required")?.GetValue(p) as bool? ?? false;
                        var locObj = p?.GetType().GetProperty("Location")?.GetValue(p);
                        var loc = Microsoft.OpenApi.ParameterLocation.Query;
                        if (locObj is Microsoft.OpenApi.ParameterLocation ml) loc = ml;
                        else if (locObj != null && locObj.ToString()?.EndsWith("Path") == true) loc = Microsoft.OpenApi.ParameterLocation.Path;

                        var newParam = new ApiForge.Domain.Models.ApiParameters.ApiParameter { Name = name, Type = type, Required = required, Location = loc };
                        if (loc == Microsoft.OpenApi.ParameterLocation.Path) endpoint.PathParameters.Add(newParam); else endpoint.QueryParameters.Add(newParam);
                    }
                }
                var requestBody = epObj?.GetType().GetProperty("RequestBody")?.GetValue(epObj);
                if (requestBody != null) endpoint.RequestBodyType = requestBody.GetType().GetProperty("ReferenceName")?.GetValue(requestBody) as string;
                group.Endpoints.Add(endpoint);
            }
            return plan;
        }
    }
}

namespace ApiForge.Infrastructure.Parser
{
    using ApiForge.Domain.Models;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;

    public class OpenApiParser : ApiForge.Application.Interfaces.IOpenApiParser
    {
        public async Task<ApiDefinition> ParseAsync(Stream stream)
        {
            using var sr = new StreamReader(stream, Encoding.UTF8, true, 1024, true);
            var txt = await sr.ReadToEndAsync();
            var def = new ApiDefinition();
            using var reader = new StringReader(txt);
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                var trimmed = line.Trim();
                if (trimmed.StartsWith("title:")) { def.Title = trimmed.Substring("title:".Length).Trim(); break; }
            }
            return def;
        }
    }
}

namespace Microsoft.AspNetCore.Mvc {
    public class ControllerBase { public ControllerContext? ControllerContext { get; set; } }
    public class ControllerContext { public Microsoft.AspNetCore.Http.DefaultHttpContext? HttpContext { get; set; } }
    public interface IActionResult { }
    public class BadRequestObjectResult : IActionResult { public object? Value { get; set; } public BadRequestObjectResult(object? v) { Value = v; } }
    public class FileContentResult : IActionResult { public byte[] FileContents { get; set; } public string ContentType { get; set; } public string FileDownloadName { get; set; } public FileContentResult(byte[] content, string contentType) { FileContents = content; ContentType = contentType; FileDownloadName = string.Empty; } }
    public class OkObjectResult : IActionResult { public OkObjectResult(object? o) { } public object? Value { get; set; } }
    public class FromFormAttribute : Attribute { }
}
// ControllerContext already declared above; avoid duplicate declaration

namespace Microsoft.AspNetCore.Http { using System.IO; public interface IFormFile { string FileName { get; } long Length { get; } Stream OpenReadStream(); } public class FormFile : IFormFile { private Stream _s; public FormFile(Stream s, long pos, long len, string name, string fileName) { _s = s; FileName = fileName; Length = len; } public string FileName { get; } public long Length { get; } public Stream OpenReadStream() => _s; } }

namespace Microsoft.AspNetCore.Http {
    public class DefaultHttpContext { public object? RequestServices { get; set; } }
}

namespace Microsoft.Extensions.FileProviders {
    public interface IFileProvider { }
    public class NullFileProvider : IFileProvider { }
}

namespace Microsoft.AspNetCore.Hosting {
    using Microsoft.Extensions.FileProviders;
    public interface IWebHostEnvironment
    {
        string EnvironmentName { get; set; }
        string ApplicationName { get; set; }
        string WebRootPath { get; set; }
        IFileProvider WebRootFileProvider { get; set; }
        string ContentRootPath { get; set; }
        IFileProvider ContentRootFileProvider { get; set; }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    using System;
    using System.Collections.Generic;

    public class ServiceCollection : List<object>
    {
        private readonly Dictionary<Type, object> _singletons = new Dictionary<Type, object>();

        public void AddSingleton<T>(T instance) where T : class
        {
            _singletons[typeof(T)] = instance!;
            this.Add(instance!);
        }

        public IServiceProvider BuildServiceProvider()
        {
            return new SimpleServiceProvider(_singletons);
        }

        private class SimpleServiceProvider : IServiceProvider
        {
            private readonly Dictionary<Type, object> _map;
            public SimpleServiceProvider(Dictionary<Type, object> map) { _map = map; }
            public object? GetService(Type serviceType)
            {
                if (_map.TryGetValue(serviceType, out var v)) return v;
                return null;
            }
        }
    }
}
