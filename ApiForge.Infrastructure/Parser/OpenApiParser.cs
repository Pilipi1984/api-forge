using ApiForge.Application.Interfaces;
using ApiForge.Domain.Enums;
using ApiForge.Domain.Models;
using ApiForge.Domain.Models.ApiParameters;
using ApiForge.Domain.Models.Schema;
using ApiForge.Infrastructure.Helpers;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Reader;

namespace ApiForge.Infrastructure.Parser
{
    /// <summary>
    /// Parses OpenAPI documents and maps them to the internal API definition model.
    /// </summary>
    public class OpenApiParser : IOpenApiParser
    {
        private const string Object = "object";
        private const string StringTypename = "string";
        private const string ArchitectureExtensionKey = "x-architecture";

        /// <summary>
        /// Tag-name substrings that, in the absence of an explicit "x-architecture" extension,
        /// weakly suggest a Hexagonal (Ports &amp; Adapters) style spec. This is a best-effort
        /// fallback only — it never overrides an explicit "x-architecture" value.
        /// </summary>
        private static readonly string[] HexagonalTagHints = { "port", "adapter", "driven", "driving" };

        /// <summary>
        /// Parses an OpenAPI document from a stream and maps it to an ApiDefinition.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns>The parsed API definition.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<ApiDefinition> ParseAsync(Stream stream)
        {
            var settings = new OpenApiReaderSettings();
            settings.AddYamlReader();

            var result = await OpenApiDocument.LoadAsync(stream, settings: settings);

            if (result.Document is null)
            {
                throw new InvalidOperationException(
                    "OpenAPI document could not be parsed: " +
                    string.Join(", ", result.Diagnostic?.Errors.Select(e => e.Message) ?? Enumerable.Empty<string>()));
            }

            return MapDefinition(result.Document);
        }

        /// <summary>
        /// Maps an OpenAPI document to an ApiDefinition.
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Returns the mapped API definition.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static ApiDefinition MapDefinition(OpenApiDocument? doc)
        {
            if (doc is null)
            {
                throw new InvalidOperationException("OpenAPI document is null.");
            }

            var definition = new ApiDefinition
            {
                Title = doc.Info?.Title ?? string.Empty,
                Version = doc.Info?.Version ?? string.Empty,
                Architecture = DetectArchitecture(doc)
            };

            // 1. Reusable models (components/schemas)
            if (doc.Components?.Schemas is not null)
            {
                foreach (var (name, schema) in doc.Components.Schemas)
                {
                    definition.Models.Add(MapModel(name, schema));
                }
            }

            // 2. Endpoints (paths)
            if (doc.Paths is not null)
            {
                foreach (var (route, pathItem) in doc.Paths)
                {
                    if (pathItem.Operations is null) 
                    {
                        continue;
                    }

                    foreach (var (method, operation) in pathItem.Operations)
                    {
                        definition.Endpoints.Add(MapEndpoint(route, method.ToString(), operation));
                    }
                }
            }

            return definition;
        }

        /// <summary>
        /// Detects the intended architecture style for the generated solution.
        /// Priority:
        /// 1. An explicit "x-architecture" vendor extension on the document root or on "info"
        ///    (accepted values: "hexagonal" / "ports-and-adapters" for Hexagonal,
        ///    "clean" / "clean-architecture" for Clean Architecture).
        /// 2. A weak fallback heuristic over tag names, only used when no explicit value is present.
        /// 3. Clean Architecture, as the safe default.
        /// </summary>
        /// <param name="doc"></param>
        /// <returns>Returns the detected architecture style.</returns>
        private static ArchitectureStyle DetectArchitecture(OpenApiDocument doc)
        {
            var explicitValue = GetExtensionStringValue(doc.Extensions, ArchitectureExtensionKey)
                ?? GetExtensionStringValue(doc.Info?.Extensions, ArchitectureExtensionKey);

            if (ArchitectureStyleParser.TryParse(explicitValue, out var explicitStyle))
            {
                return explicitStyle;
            }

            var tagNames = doc.Tags?.Select(t => t.Name ?? string.Empty) ?? Enumerable.Empty<string>();
            var looksHexagonal = tagNames.Any(tag =>
                HexagonalTagHints.Any(hint => tag.Contains(hint, StringComparison.OrdinalIgnoreCase)));

            return looksHexagonal ? ArchitectureStyle.Hexagonal : ArchitectureStyle.CleanArchitecture;
        }

        /// <summary>
        /// Reads a vendor extension value as plain text, if present. Handles the JSON-backed
        /// extension representation used by Microsoft.OpenApi 3.x (System.Text.Json-based nodes).
        /// </summary>
        /// <param name="extensions"></param>
        /// <param name="key"></param>
        /// <returns>Returns the extension's raw string value, or null if not present.</returns>
        private static string? GetExtensionStringValue(IDictionary<string, IOpenApiExtension>? extensions, string key)
        {
            if (extensions is null || !extensions.TryGetValue(key, out var extension) || extension is null)
            {
                return null;
            }

            // Microsoft.OpenApi 3.x represents JSON-backed extensions as JsonNodeExtension,
            // exposing the raw value through its Node property. Fall back to ToString() for
            // any other IOpenApiExtension implementation.
            if (extension is JsonNodeExtension jsonExtension)
            {
                return jsonExtension.Node?.ToString().Trim('"');
            }

            return extension.ToString()?.Trim('"');
        }

        /// <summary>
        /// Maps an OpenAPI operation to an ApiEndpoint.
        /// </summary>
        /// <param name="route"></param>
        /// <param name="httpMethod"></param>
        /// <param name="operation"></param>
        /// <returns>Returns the mapped API endpoint.</returns>
        private static ApiEndpoint MapEndpoint(string route, string httpMethod, OpenApiOperation operation)
        {
            var endpoint = new ApiEndpoint
            {
                Route = route,
                HttpMethod = httpMethod,
                OperationId = operation.OperationId ?? string.Empty,
                Summary = operation.Summary ?? string.Empty,
            };

            foreach (var parameter in operation.Parameters ?? Enumerable.Empty<IOpenApiParameter>())
            {
                endpoint.Parameters.Add(MapParameter(parameter));
            }

            var requestSchema = operation.RequestBody?.Content?
                .Values.FirstOrDefault()?.Schema;
            if (requestSchema is not null)
                endpoint.RequestBody = MapSchema(requestSchema);

            var responseSchema = operation.Responses?
                .Values.FirstOrDefault()?.Content?
                .Values.FirstOrDefault()?.Schema;
            if (responseSchema is not null)
                endpoint.Response = MapSchema(responseSchema);

            return endpoint;
        }

        /// <summary>
        /// Maps an OpenAPI parameter to an ApiParameter.
        /// </summary>
        /// <param name="p"></param>
        /// <returns>Returns the mapped API parameter.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        private static ApiParameter MapParameter(IOpenApiParameter p)
        {
            if (p.Schema is null)
            {
                throw new InvalidOperationException("Schema parameter is null.");
            }

            var schemaType = MapPrimitiveClrType(p.Schema.Type, p.Schema.Format);

            if (p.Name is null)
            {
                throw new InvalidOperationException("Name parameter is null.");
            }

            return p.In switch
            {
                ParameterLocation.Path => new ApiPathParameter
                {
                    Name = p.Name,
                    Type = schemaType,
                    Location = ParameterLocation.Path,
                    Required = p.Required,
                    Description = p.Description
                },
                ParameterLocation.Query => new ApiQueryParameter
                {
                    Name = p.Name,
                    Type = schemaType,
                    Location = ParameterLocation.Query,
                    Required = p.Required,
                    Description = p.Description,
                    Explode = p.Explode
                },
                ParameterLocation.Header => new ApiHeaderParameter
                {
                    Name = p.Name,
                    Type = schemaType,
                    Location = ParameterLocation.Header,
                    Required = p.Required,
                    Description = p.Description
                },
                ParameterLocation.Cookie => new ApiCookieParameter
                {
                    Name = p.Name,
                    Type = schemaType,
                    Location = ParameterLocation.Cookie,
                    Required = p.Required,
                    Description = p.Description
                },
                _ => throw new NotSupportedException($"Location not supported: {p.In}")
            };
        }

        /// <summary>
        /// Maps an OpenAPI schema to an ApiSchema.
        /// </summary>
        /// <param name="schema"></param>
        /// <returns>Returns the mapped API schema.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        private static ApiSchema MapSchema(IOpenApiSchema schema)
        {
            // 1. Reference to a reusable schema
            if (schema is OpenApiSchemaReference reference)
            {
                if (reference.Reference is null)
                {
                    throw new InvalidOperationException("Reference is null.");
                }

                if (reference.Reference.Id is null)
                {
                    throw new InvalidOperationException("Reference ID es null.");
                }

                return new ReferenceSchema
                {
                    ReferenceName = reference.Reference.Id,
                    OpenApiType = Object,
                    ClrType = reference.Reference.Id
                };
            }

            // 2. Enum
            if (schema.Enum is { Count: > 0 })
            {
                return new EnumSchema
                {
                    OpenApiType = schema.Type?.ToString() ?? StringTypename,
                    ClrType = schema.Type?.ToString() ?? StringTypename,
                    Values = schema.Enum.Select(v => v.ToString() ?? string.Empty).ToList(),
                    Nullable = OpenApiHelper.GetNullable(schema),
                    Description = schema.Description
                };
            }

            // 3. Array
            if (schema.Type == JsonSchemaType.Array && schema.Items is not null)
            {
                return new ArraySchema
                {
                    OpenApiType = "array",
                    ClrType = "array",
                    ItemSchema = MapSchema(schema.Items),
                    Nullable = OpenApiHelper.GetNullable(schema),
                    Description = schema.Description
                };
            }

            // 4. Objeto
            if (schema.Type == JsonSchemaType.Object || schema.Properties?.Count > 0)
            {
                return new ObjectSchema
                {
                    OpenApiType = Object,
                    ClrType = Object,
                    Properties = schema.Properties?
                        .Select(kv => MapProperty(kv.Key, kv.Value, schema.Required))
                        .ToList() ?? new List<ApiProperty>(),
                    Nullable = OpenApiHelper.GetNullable(schema),
                    Description = schema.Description
                };
            }

            // 5. Primitive (string, integer, number, boolean)
            return new PrimitiveSchema
            {
                OpenApiType = schema.Type?.ToString() ?? StringTypename,
                ClrType = MapPrimitiveClrType(schema.Type, schema.Format),
                Nullable = OpenApiHelper.GetNullable(schema),
                Description = schema.Description
            };
        }

        /// <summary>
        /// Maps a primitive OpenAPI type and format to a corresponding C# type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="format"></param>
        /// <returns>Returns the mapped C# type.</returns>
        private static string MapPrimitiveClrType(JsonSchemaType? type, string? format) => (type, format) switch
        {
            (JsonSchemaType.String, "date-time") => "DateTimeOffset",
            (JsonSchemaType.String, "date") => "DateOnly",
            (JsonSchemaType.String, "uuid") => "Guid",
            (JsonSchemaType.String, _) => StringTypename,
            (JsonSchemaType.Integer, "int64") => "long",
            (JsonSchemaType.Integer, _) => "int",
            (JsonSchemaType.Number, "float") => "float",
            (JsonSchemaType.Number, _) => "double",
            (JsonSchemaType.Boolean, _) => "bool",
            _ => Object
        };

        /// <summary>
        /// Maps an OpenAPI schema to an ApiModel.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="schema"></param>
        /// <returns>Returns the mapped API model.</returns>
        private static ApiModel MapModel(string name, IOpenApiSchema schema)
        {
            return new ApiModel
            {
                Name = name,
                Properties = schema.Properties?
                    .Select(kv => MapProperty(kv.Key, kv.Value, schema.Required))
                    .ToList() ?? []
            };
        }

        /// <summary>
        /// Maps an OpenAPI schema property to an ApiProperty.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="schema"></param>
        /// <param name="required"></param>
        /// <returns>Returns the mapped API property.</returns>
        private static ApiProperty MapProperty(string name, IOpenApiSchema schema, ISet<string>? required)
        {
            return new ApiProperty
            {
                Name = name,
                Type = MapPrimitiveClrType(schema.Type, schema.Format),
                Required = required?.Contains(name) ?? false,
                Nullable = OpenApiHelper.GetNullable(schema),
                Format = schema.Format,
                Description = schema.Description,
                DefaultValue = schema.Default
            };
        }
    }
}
