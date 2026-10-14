using ApiForge.Domain.Models.ApiParameters;
using ApiForge.Infrastructure.Generator.Planning;
using ApiForge.Infrastructure.Helpers;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

namespace ApiForge.Tests.Helpers
{
    /// <summary>
    /// Tests for the OpenApiHelper class.
    /// </summary>
    public class OpenApiHelperTests
    {
        /// <summary>
        /// Tests that GetNullable returns false when the schema is null.
        /// </summary>
        [Fact]
        public void GetNullable_NullSchema_ReturnsFalse() =>
            Assert.False(OpenApiHelper.GetNullable(null!));

        /// <summary>
        /// Tests that GetNullable returns true when the schema type is null.
        /// </summary>
        [Fact]
        public void GetNullable_TypeNull_ReturnsTrue() =>
            Assert.True(OpenApiHelper.GetNullable(new OpenApiSchema { Type = JsonSchemaType.Null }));

        /// <summary>
        /// Tests that GetNullable returns false when the schema has no keywords.
        /// </summary>
        [Fact]
        public void GetNullable_NoKeywords_ReturnsFalse() =>
            Assert.False(OpenApiHelper.GetNullable(new OpenApiSchema { Type = JsonSchemaType.String }));

        /// <summary>
        /// Tests that GetNullable returns the correct value based on the "nullable" keyword in the schema.
        /// </summary>
        /// <param name="raw"></param>
        /// <param name="expected"></param>
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

    /// <summary>
    /// Tests for the GeneratedFilePathHelper class.
    /// </summary>
    public class GeneratedFilePathHelperTests
    {
        /// <summary>
        /// Tests that BuildRelativePath returns the expected relative path based on the project namespace, full namespace, and file name.
        /// </summary>
        /// <param name="project"></param>
        /// <param name="full"></param>
        /// <param name="file"></param>
        /// <param name="expected"></param>
        [Theory]
        [InlineData("Acme.Domain", "Acme.Domain", "A.cs", "Acme.Domain/A.cs")]
        [InlineData("Acme.Domain", "Acme.Domain.Models", "A.cs", "Acme.Domain/Models/A.cs")]
        [InlineData("Acme.Domain", "Acme.Domain.Models.Sub", "A.cs", "Acme.Domain/Models/Sub/A.cs")]
        [InlineData("Acme.Domain", "Other.Namespace", "A.cs", "Acme.Domain/A.cs")]
        public void BuildRelativePath_Works(string project, string full, string file, string expected) =>
            Assert.Equal(expected, GeneratedFilePathHelper.BuildRelativePath(project, full, file));
    }

    /// <summary>
    /// Tests for the QualifiedNameResolver class.
    /// </summary>
    public class QualifiedNameResolverExtraTests
    {
        /// <summary>
        /// Tests that Resolve returns "Unnamed" when the input is blank or contains only dots.
        /// </summary>
        /// <param name="input"></param>
        [Theory]
        [InlineData("   ")]
        [InlineData("...")]        
        public void Resolve_BlankOrOnlyDots_ReturnsUnnamed(string input)
        {
            var res = QualifiedNameResolver.Resolve(input);

            Assert.Equal("Unnamed", res.ClassName);
            Assert.Empty(res.NamespaceSegments);
        }

        /// <summary>
        /// Tests that Resolve correctly resolves a single segment name with no namespace.
        /// </summary>
        [Fact]
        public void Resolve_SingleSegment_HasNoNamespace()
        {
            var res = QualifiedNameResolver.Resolve("order");

            Assert.Equal("Order", res.ClassName);
            Assert.Empty(res.NamespaceSegments);
        }

        /// <summary>
        /// Tests that Resolve correctly resolves a name with multiple segments, extracting the class name and namespace segments.
        /// </summary>
        [Fact]
        public void BuildNamespace_AppendsSegments() =>
            Assert.Equal("Root.Models.A.B", QualifiedNameResolver.BuildNamespace("Root.Models", ["A", "B"]));

        /// <summary>
        /// Tests that BuildNamespace returns the base namespace when no additional segments are provided.
        /// </summary>
        [Fact]
        public void BuildNamespace_NoSegments_ReturnsBase() =>
            Assert.Equal("Root.Models", QualifiedNameResolver.BuildNamespace("Root.Models", []));

        /// <summary>
        /// Tests that BuildFolderPath correctly constructs a folder path from the provided namespace segments.
        /// </summary>
        [Fact]
        public void BuildFolderPath_Works()
        {
            Assert.Equal("/A/B", QualifiedNameResolver.BuildFolderPath(["A", "B"]));
            Assert.Equal(string.Empty, QualifiedNameResolver.BuildFolderPath([]));
        }
    }

    /// <summary>
    /// Tests for the EndpointSignatureHelper class.
    /// </summary>
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

        /// <summary>
        /// Tests that BuildParameterList returns an empty string when there are no parameters in the endpoint plan.
        /// </summary>
        [Fact]
        public void BuildParameterList_NoParameters_ReturnsEmpty() =>
            Assert.Equal(string.Empty, EndpointSignatureHelper.BuildParameterList(Plan()));

        /// <summary>
        /// Tests that BuildParameterList correctly orders parameters from path, query, header, and body in the resulting parameter list string.
        /// </summary>
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