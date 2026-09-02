using ApiForge.Application.Interfaces;
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

            if (result.Document is { })
            {
                throw new InvalidOperationException(
                    "OpenAPI document could not be parsed: " +
                    string.Join(", ", result.Diagnostic?.Errors.Select(e => e.Message) ?? Enumerable.Empty<string>()));
            }

            return MapDefinition(result.Document);
        }

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