using ApiForge.Domain.Models;
using ApiForge.Domain.Models.Schema;
using ApiForge.Generator;
using ApiForge.Infrastructure.Generator;
using ApiForge.Infrastructure.Generator.Planning;
using ApiForge.Infrastructure.Generator.Resolvers;
using System.Text;

namespace ApiForge.Tests.Generators
{
    public class DomainModelGeneratorExtraTests
    {
        private static string Generate(ApiProperty property)
        {
            var ns = ProjectNamespaces.From("Root", ArchitectureConventions.For(Domain.Enums.ArchitectureStyle.Clean));
            var definition = new ApiDefinition
            {
                Models = [new ApiModel { Name = "Pet", Properties = [property] }]
            };

            return Assert.Single(DomainModelGenerator.Generate(definition, ns)).Content;
        }

        [Fact]
        public void Generate_EscapesXmlInDescriptions()
        {
            var content = Generate(new ApiProperty { Name = "Note", Type = "string", Description = "a < b & c" });

            Assert.Contains("/// a &lt; b &amp; c", content);
        }

        [Fact]
        public void Generate_NullableString_HasNoDefaultAssignment()
        {
            var content = Generate(new ApiProperty { Name = "nick", Type = "string", Nullable = true });

            Assert.Contains("public string? Nick { get; set; }", content);
            Assert.DoesNotContain("string.Empty", content);
        }

        [Fact]
        public void Generate_UsesModelsNamespaceAndPascalCaseProperties()
        {
            var content = Generate(new ApiProperty { Name = "firstName", Type = "string" });

            Assert.Contains("namespace Root.Domain.Models", content);
            Assert.Contains("public string FirstName { get; set; } = string.Empty;", content);
        }
    }

    public class CSharpTypeResolverExtraTests
    {
        private const string Ns = "Root.Models";

        [Fact]
        public void Resolve_Null_ReturnsObject() =>
            Assert.Equal("object", CSharpTypeResolver.Resolve(null, Ns));

        [Fact]
        public void Resolve_ObjectSchema_ReturnsObject() =>
            Assert.Equal("object", CSharpTypeResolver.Resolve(new ObjectSchema { OpenApiType = "object", ClrType = "object" }, Ns));

        [Fact]
        public void Resolve_NullablePrimitive_AppendsQuestionMark() =>
            Assert.Equal("int?", CSharpTypeResolver.Resolve(
                new PrimitiveSchema { OpenApiType = "integer", ClrType = "Int32", Nullable = true }, Ns));

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

        [Fact]
        public void ResolveProperty_NonNullable_HasNoSuffix() =>
            Assert.Equal("int", CSharpTypeResolver.ResolveProperty(new ApiProperty { Name = "Id", Type = "Int32" }));

        [Fact]
        public void NormalizePrimitive_UnknownType_PassesThrough() =>
            Assert.Equal("DateTimeOffset", CSharpTypeResolver.NormalizePrimitive("DateTimeOffset"));
    }

    public class ApiForgeGeneratorDefaultsTests
    {
        private const string Yaml = "openapi: 3.0.0\ninfo:\n  title: Shop\n  version: 1.0.0\npaths: {}";

        [Fact]
        public void Ctor_NullParser_Throws() =>
            Assert.Throws<ArgumentNullException>(() => new ApiForgeGenerator(null!, new CodeGenerator()));

        [Fact]
        public void Ctor_NullGenerator_Throws() =>
            Assert.Throws<ArgumentNullException>(() => new ApiForgeGenerator(new Infrastructure.Parser.OpenApiParser(), null!));

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