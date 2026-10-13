using ApiForge.Domain.Models.ApiParameters;
using ApiForge.Infrastructure.Generator.Planning;
using ApiForge.Infrastructure.Helpers;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

namespace ApiForge.Tests.Helpers
{
    public class OpenApiHelperTests
    {
        [Fact]
        public void GetNullable_NullSchema_ReturnsFalse() =>
            Assert.False(OpenApiHelper.GetNullable(null!));

        [Fact]
        public void GetNullable_TypeNull_ReturnsTrue() =>
            Assert.True(OpenApiHelper.GetNullable(new OpenApiSchema { Type = JsonSchemaType.Null }));

        [Fact]
        public void GetNullable_NoKeywords_ReturnsFalse() =>
            Assert.False(OpenApiHelper.GetNullable(new OpenApiSchema { Type = JsonSchemaType.String }));

        [Theory]
        [InlineData("true", true)]
        [InlineData("1", true)]
        [InlineData("false", false)]
        [InlineData("0", false)]
        public void GetNullable_NullableKeyword(string raw, bool expected)
        {
            var schema = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                UnrecognizedKeywords = new Dictionary<string, JsonNode> { ["nullable"] = JsonNode.Parse(raw)! }
            };

            Assert.Equal(expected, OpenApiHelper.GetNullable(schema));
        }
    }

    public class GeneratedFilePathHelperTests
    {
        [Theory]
        [InlineData("Acme.Domain", "Acme.Domain", "A.cs", "Acme.Domain/A.cs")]
        [InlineData("Acme.Domain", "Acme.Domain.Models", "A.cs", "Acme.Domain/Models/A.cs")]
        [InlineData("Acme.Domain", "Acme.Domain.Models.Sub", "A.cs", "Acme.Domain/Models/Sub/A.cs")]
        [InlineData("Acme.Domain", "Other.Namespace", "A.cs", "Acme.Domain/A.cs")]
        public void BuildRelativePath_Works(string project, string full, string file, string expected) =>
            Assert.Equal(expected, GeneratedFilePathHelper.BuildRelativePath(project, full, file));
    }

    public class QualifiedNameResolverExtraTests
    {
        [Theory]
        [InlineData("   ")]
        [InlineData("...")]
        public void Resolve_BlankOrOnlyDots_ReturnsUnnamed(string input)
        {
            var res = QualifiedNameResolver.Resolve(input);

            Assert.Equal("Unnamed", res.ClassName);
            Assert.Empty(res.NamespaceSegments);
        }

        [Fact]
        public void Resolve_SingleSegment_HasNoNamespace()
        {
            var res = QualifiedNameResolver.Resolve("order");

            Assert.Equal("Order", res.ClassName);
            Assert.Empty(res.NamespaceSegments);
        }

        [Fact]
        public void BuildNamespace_AppendsSegments() =>
            Assert.Equal("Root.Models.A.B", QualifiedNameResolver.BuildNamespace("Root.Models", ["A", "B"]));

        [Fact]
        public void BuildNamespace_NoSegments_ReturnsBase() =>
            Assert.Equal("Root.Models", QualifiedNameResolver.BuildNamespace("Root.Models", []));

        [Fact]
        public void BuildFolderPath_Works()
        {
            Assert.Equal("/A/B", QualifiedNameResolver.BuildFolderPath(["A", "B"]));
            Assert.Equal(string.Empty, QualifiedNameResolver.BuildFolderPath([]));
        }
    }

    public class EndpointSignatureHelperTests
    {
        private static ParameterPlan P(ApiParameter source, string type) =>
            new() { Name = source.Name, CSharpType = type, Source = source };

        private static EndpointPlan Plan(
            IReadOnlyList<ParameterPlan>? path = null,
            IReadOnlyList<ParameterPlan>? query = null,
            IReadOnlyList<ParameterPlan>? header = null,
            string? bodyType = null) => new()
            {
                MethodName = "M",
                HttpMethod = "GET",
                Route = "/x",
                ReturnType = "object",
                PathParameters = path ?? [],
                QueryParameters = query ?? [],
                HeaderParameters = header ?? [],
                RequestBodyType = bodyType,
                RequestBodyParameterName = bodyType is null ? null : "request"
            };

        [Fact]
        public void BuildParameterList_NoParameters_ReturnsEmpty() =>
            Assert.Equal(string.Empty, EndpointSignatureHelper.BuildParameterList(Plan()));

        [Fact]
        public void BuildParameterList_OrdersPathQueryHeaderBody()
        {
            var id = new ApiPathParameter { Name = "id", Type = "int", Location = ParameterLocation.Path };
            var q = new ApiQueryParameter { Name = "limit", Type = "int", Location = ParameterLocation.Query };
            var h = new ApiHeaderParameter { Name = "traceId", Type = "string", Location = ParameterLocation.Header };

            var result = EndpointSignatureHelper.BuildParameterList(
                Plan([P(id, "int")], [P(q, "int?")], [P(h, "string?")], "Pet"));

            Assert.Equal("int id, int? limit, string? traceId, Pet request, ", result);
        }
    }
}