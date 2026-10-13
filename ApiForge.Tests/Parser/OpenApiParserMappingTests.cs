using ApiForge.Domain.Enums;
using ApiForge.Domain.Models;
using ApiForge.Domain.Models.ApiParameters;
using ApiForge.Domain.Models.Schema;
using ApiForge.Infrastructure.Parser;
using System.Text;

namespace ApiForge.Tests.Parser
{
    public class OpenApiParserMappingTests
    {
        private const string Spec = """
            openapi: 3.0.0
            info:
              title: Shop
              version: 2.1.0
            paths:
              /orders/{orderId}:
                get:
                  operationId: getOrder
                  summary: Gets an order
                  parameters:
                    - name: orderId
                      in: path
                      required: true
                      schema:
                        type: integer
                        format: int64
                    - name: verbose
                      in: query
                      schema:
                        type: boolean
                    - name: traceId
                      in: header
                      schema:
                        type: string
                    - name: session
                      in: cookie
                      schema:
                        type: string
                  responses:
                    '200':
                      description: ok
                      content:
                        application/json:
                          schema:
                            $ref: '#/components/schemas/Order'
              /orders:
                post:
                  operationId: createOrder
                  requestBody:
                    content:
                      application/json:
                        schema:
                          type: object
                          properties:
                            note:
                              type: string
                  responses:
                    '201':
                      description: created
                      content:
                        application/json:
                          schema:
                            type: array
                            items:
                              $ref: '#/components/schemas/Order'
              /status:
                get:
                  responses:
                    '200':
                      description: ok
                      content:
                        text/plain:
                          schema:
                            type: string
                            enum: [up, down]
              /count:
                get:
                  responses:
                    '200':
                      description: ok
                      content:
                        text/plain:
                          schema:
                            type: integer
            components:
              schemas:
                Order:
                  type: object
                  required: [id]
                  properties:
                    id:
                      type: string
                      format: uuid
                    createdAt:
                      type: string
                      format: date-time
                      description: Creation date
                    total:
                      type: number
                      format: float
            """;

        private static async Task<ApiDefinition> ParseAsync(string yaml)
        {
            using var ms = new MemoryStream(Encoding.UTF8.GetBytes(yaml));
            return await new OpenApiParser().ParseAsync(ms);
        }

        private static string Minimal(string extra) =>
            $"openapi: 3.0.0\n{extra}\ninfo:\n  title: A\n  version: '1'\npaths: {{}}";

        [Fact]
        public async Task ParseAsync_MapsInfoAndDefaultsToCleanArchitecture()
        {
            var def = await ParseAsync(Spec);

            Assert.Equal("Shop", def.Title);
            Assert.Equal("2.1.0", def.Version);
            Assert.Equal(ArchitectureStyle.Clean, def.Architecture);
            Assert.Equal(4, def.Endpoints.Count);
        }

        [Fact]
        public async Task ParseAsync_MapsComponentSchemasToModels()
        {
            var def = await ParseAsync(Spec);

            var model = Assert.Single(def.Models);
            Assert.Equal("Order", model.Name);
            Assert.Equal(3, model.Properties.Count);

            var id = model.Properties.Single(p => p.Name == "id");
            Assert.True(id.Required);
            Assert.Equal("Guid", id.Type);

            var created = model.Properties.Single(p => p.Name == "createdAt");
            Assert.False(created.Required);
            Assert.Equal("DateTimeOffset", created.Type);
            Assert.Equal("Creation date", created.Description);

            Assert.Equal("float", model.Properties.Single(p => p.Name == "total").Type);
        }

        [Fact]
        public async Task ParseAsync_MapsEndpointParametersByLocation()
        {
            var def = await ParseAsync(Spec);
            var ep = def.Endpoints.Single(e => e.OperationId == "getOrder");

            Assert.Equal("/orders/{orderId}", ep.Route);
            Assert.Equal("GET", ep.HttpMethod, true);
            Assert.Equal("Gets an order", ep.Summary);

            var path = Assert.Single(ep.Parameters.OfType<ApiPathParameter>());
            Assert.Equal("long", path.Type);
            Assert.True(path.Required);

            Assert.Equal("bool", Assert.Single(ep.Parameters.OfType<ApiQueryParameter>()).Type);
            Assert.Equal("string", Assert.Single(ep.Parameters.OfType<ApiHeaderParameter>()).Type);
            Assert.Single(ep.Parameters.OfType<ApiCookieParameter>());
        }

        [Fact]
        public async Task ParseAsync_MapsReferenceResponse()
        {
            var def = await ParseAsync(Spec);
            var ep = def.Endpoints.Single(e => e.OperationId == "getOrder");

            var response = Assert.IsType<ReferenceSchema>(ep.Response);
            Assert.Equal("Order", response.ReferenceName);
        }

        [Fact]
        public async Task ParseAsync_MapsInlineObjectRequestBodyAndArrayResponse()
        {
            var def = await ParseAsync(Spec);
            var ep = def.Endpoints.Single(e => e.OperationId == "createOrder");

            var body = Assert.IsType<ObjectSchema>(ep.RequestBody);
            Assert.Equal("note", Assert.Single(body.Properties).Name);

            var array = Assert.IsType<ArraySchema>(ep.Response);
            Assert.IsType<ReferenceSchema>(array.ItemSchema);
        }

        [Fact]
        public async Task ParseAsync_MapsEnumAndPrimitiveResponses()
        {
            var def = await ParseAsync(Spec);

            var status = def.Endpoints.Single(e => e.Route == "/status");
            Assert.Equal(string.Empty, status.OperationId);
            var enumSchema = Assert.IsType<EnumSchema>(status.Response);
            Assert.Equal(2, enumSchema.Values.Count);

            var count = def.Endpoints.Single(e => e.Route == "/count");
            var primitive = Assert.IsType<PrimitiveSchema>(count.Response);
            Assert.Equal("int", primitive.ClrType);
        }

        [Theory]
        [InlineData("x-architecture: hexagonal", ArchitectureStyle.Hexagonal)]
        [InlineData("x-architecture: ports-and-adapters", ArchitectureStyle.Hexagonal)]
        [InlineData("x-architecture: clean", ArchitectureStyle.Clean)]
        [InlineData("x-architecture: clean-architecture", ArchitectureStyle.Clean)]
        public async Task ParseAsync_ReadsRootXArchitectureExtension(string extension, ArchitectureStyle expected)
        {
            var def = await ParseAsync(Minimal(extension));

            Assert.Equal(expected, def.Architecture);
        }

        [Fact]
        public async Task ParseAsync_ReadsXArchitectureFromInfo()
        {
            var yaml = "openapi: 3.0.0\ninfo:\n  title: A\n  version: '1'\n  x-architecture: hexagonal\npaths: {}";

            var def = await ParseAsync(yaml);

            Assert.Equal(ArchitectureStyle.Hexagonal, def.Architecture);
        }

        [Fact]
        public async Task ParseAsync_UsesTagHeuristic_WhenNoExtension()
        {
            var def = await ParseAsync(Minimal("tags:\n  - name: Driven-Adapters"));

            Assert.Equal(ArchitectureStyle.Hexagonal, def.Architecture);
        }

        [Fact]
        public async Task ParseAsync_ExplicitExtension_WinsOverTagHeuristic()
        {
            var def = await ParseAsync(Minimal("x-architecture: clean\ntags:\n  - name: adapter"));

            Assert.Equal(ArchitectureStyle.Clean, def.Architecture);
        }
    }
}