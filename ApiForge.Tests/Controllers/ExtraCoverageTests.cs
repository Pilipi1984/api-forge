using ApiForge.Domain.Models;
using ApiForge.Domain.Models.Schema;
using ApiForge.Generator;
using ApiForge.Infrastructure.Generator;
using ApiForge.Infrastructure.Generator.Planning;
using ApiForge.Infrastructure.Generator.Resolvers;
using System.Text;

namespace ApiForge.Tests.Generators
{
    /// <summary>
    /// Tests for the DomainModelGenerator class, which generates domain model classes from API definitions.
    /// </summary>
    public class DomainModelGeneratorExtraTests
    {
        /// <summary>
        /// Generates a domain model class for a single property and returns the generated content.
        /// </summary>
        /// <param name="property">The API property for which to generate a domain model class.</param>
        /// <returns>The generated domain model class content.</returns>
        private static string Generate(ApiProperty property)
        {
            var ns = ProjectNamespaces.From("Root", ArchitectureConventions.For(Domain.Enums.ArchitectureStyle.Clean));
            var definition = new ApiDefinition
            {
                Models = [new ApiModel { Name = "Pet", Properties = [property] }]
            };

            return Assert.Single(DomainModelGenerator.Generate(definition, ns)).Content;
        }

        /// <summary>
        /// Tests that the generated domain model class correctly escapes XML characters in property descriptions.
        /// </summary>
        [Fact]
        public void Generate_EscapesXmlInDescriptions()
        {
            var content = Generate(new ApiProperty { Name = "Note", Type = "string", Description = "a < b & c" });

            Assert.Contains("/// a &lt; b &amp; c", content);
        }

        /// <summary>
        /// Tests that the generated domain model class for a nullable string property does not include a default assignment.
        /// </summary>
        [Fact]
        public void Generate_NullableString_HasNoDefaultAssignment()
        {
            var content = Generate(new ApiProperty { Name = "nick", Type = "string", Nullable = true });

            Assert.Contains("public string? Nick { get; set; }", content);
            Assert.DoesNotContain("string.Empty", content);
        }

        /// <summary>
        /// Tests that the generated domain model class uses the correct namespace and PascalCase for property names.
        /// </summary>
        [Fact]
        public void Generate_UsesModelsNamespaceAndPascalCaseProperties()
        {
            var content = Generate(new ApiProperty { Name = "firstName", Type = "string" });

            Assert.Contains("namespace Root.Domain.Models", content);
            Assert.Contains("public string FirstName { get; set; } = string.Empty;", content);
        }
    }

    /// <summary>
    /// Tests for the CSharpTypeResolver class, which resolves C# types from API schemas and properties.
    /// </summary>
    public class CSharpTypeResolverExtraTests
    {
        private const string Ns = "Root.Models";

        /// <summary>
        /// Tests that resolving a null schema returns the "object" type.
        /// </summary>
        [Fact]
        public void Resolve_Null_ReturnsObject() =>
            Assert.Equal("object", CSharpTypeResolver.Resolve(null, Ns));

        /// <summary>
        /// Tests that resolving an ObjectSchema returns the "object" type.
        /// </summary>
        [Fact]
        public void Resolve_ObjectSchema_ReturnsObject() =>
            Assert.Equal("object", CSharpTypeResolver.Resolve(new ObjectSchema { OpenApiType = "object", ClrType = "object" }, Ns));

        /// <summary>
        /// Tests that resolving a nullable primitive schema appends a question mark to the type.
        /// </summary>
        [Fact]
        public void Resolve_NullablePrimitive_AppendsQuestionMark() =>
            Assert.Equal("int?", CSharpTypeResolver.Resolve(
                new PrimitiveSchema { OpenApiType = "integer", ClrType = "Int32", Nullable = true }, Ns));

        /// <summary>
        /// Tests that resolving a nullable array schema appends a question mark to the type.
        /// </summary>
        [Fact]
        public void Resolve_NullableArray_AppendsQuestionMark()
        {
            var array = new ArraySchema
            {
                OpenApiType = "array",
                ClrType = "array",
                Nullable = true,
                ItemSchema = new PrimitiveSchema { OpenApiType = "integer", ClrType = "Int32" }
            };

            Assert.Equal("List<int>?", CSharpTypeResolver.Resolve(array, Ns));
        }

        /// <summary>
        /// Tests that resolving a non-nullable property returns the correct C# type without a question mark.
        /// </summary>
        [Fact]
        public void ResolveProperty_NonNullable_HasNoSuffix() =>
            Assert.Equal("int", CSharpTypeResolver.ResolveProperty(new ApiProperty { Name = "Id", Type = "Int32" }));

        /// <summary>
        /// Tests that resolving a nullable property returns the correct C# type with a question mark suffix.
        /// </summary>
        [Fact]
        public void ResolveProperty_Nullable_HasQuestionMarkSuffix() =>
            Assert.Equal("int?", CSharpTypeResolver.ResolveProperty(new ApiProperty { Name = "Id", Type = "Int32", Nullable = true }));

        /// <summary>
        /// Tests that normalizing a primitive type that is not recognized passes it through unchanged.
        /// </summary>
        [Fact]
        public void NormalizePrimitive_UnknownType_PassesThrough() =>
            Assert.Equal("DateTimeOffset", CSharpTypeResolver.NormalizePrimitive("DateTimeOffset"));
    }

    /// <summary>
    /// Tests for the ApiForgeGenerator class, which generates code and solutions from OpenAPI specifications.
    /// </summary>
    public class ApiForgeGeneratorDefaultsTests
    {
        private const string Yaml = "openapi: 3.0.0\ninfo:\n  title: Shop\n  version: 1.0.0\npaths: {}";

        /// <summary>
        /// Tests that constructing an ApiForgeGenerator with a null parser throws an ArgumentNullException.
        /// </summary>
        [Fact]
        public void Ctor_NullParser_Throws() =>
            Assert.Throws<ArgumentNullException>(() => new ApiForgeGenerator(null!, new CodeGenerator()));

        /// <summary>
        /// Tests that constructing an ApiForgeGenerator with a null code generator throws an ArgumentNullException.
        /// </summary>
        [Fact]
        public void Ctor_NullGenerator_Throws() =>
            Assert.Throws<ArgumentNullException>(() => new ApiForgeGenerator(new Infrastructure.Parser.OpenApiParser(), null!));

        /// <summary>
        /// Tests that the default constructor of ApiForgeGenerator generates a solution and a zip archive from a given OpenAPI specification.
        /// </summary>
        [Fact]
        public async Task DefaultCtor_GeneratesSolutionAndZip()
        {
            var generator = new ApiForgeGenerator();

            using var spec1 = new MemoryStream(Encoding.UTF8.GetBytes(Yaml));
            var solution = await generator.GenerateAsync(spec1, Domain.Enums.ArchitectureStyle.Hexagonal);

            using var spec2 = new MemoryStream(Encoding.UTF8.GetBytes(Yaml));
            var zip = await generator.GenerateZipAsync(spec2);

            Assert.Equal("Shop", solution.Name);
            Assert.NotEmpty(solution.Files);
            Assert.Equal("Shop", zip.Name);
            Assert.NotEmpty(zip.Content);
        }
    }
}