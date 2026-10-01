using ApiForge.Domain.Models;
using ApiForge.Domain.Models.Schema;
using ApiForge.Infrastructure.Generator.Resolvers;
using Xunit;

namespace ApiForge.Tests.Resolvers
{
    public class CSharpTypeResolverTests
    {
        [Theory]
        [InlineData("String", "string")]
        [InlineData("Boolean", "bool")]
        [InlineData("Int32", "int")]
        [InlineData("Int64", "long")]
        [InlineData("Double", "double")]
        [InlineData("Single", "float")]
        [InlineData(null, "object")]
        public void NormalizePrimitive_MapsTypes(string? input, string expected)
        {
            var actual = CSharpTypeResolver.NormalizePrimitive(input);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ResolveProperty_Nullable_AppendsQuestion()
        {
            var prop = new ApiProperty { Name = "Id", Type = "Int32", Nullable = true };
            var type = CSharpTypeResolver.ResolveProperty(prop);
            Assert.Equal("int?", type);
        }

        [Fact]
        public void Resolve_Array_ProducesList()
        {
            var array = new ApiForge.Domain.Models.Schema.ArraySchema { OpenApiType = "array", ClrType = "List", ItemSchema = new ApiForge.Domain.Models.Schema.PrimitiveSchema { OpenApiType = "integer", ClrType = "Int32" } };
            var actual = CSharpTypeResolver.Resolve(array, "Root.Models");
            Assert.Equal("List<int>", actual);
        }

        [Fact]
        public void Resolve_Enum_IsString_WithNullable()
        {
            var e = new ApiForge.Domain.Models.Schema.EnumSchema { OpenApiType = "string", ClrType = "String", Nullable = true };
            var actual = CSharpTypeResolver.Resolve(e, "Root.Models");
            Assert.Equal("string?", actual);
        }

        [Fact]
        public void Resolve_Reference_ResolvesFullName()
        {
            var r = new ApiForge.Domain.Models.Schema.ReferenceSchema { OpenApiType = "ref", ClrType = "Ref", ReferenceName = "Company.Product.ClassName" };
            var actual = CSharpTypeResolver.Resolve(r, "Root.Models");
            Assert.Equal("Root.Models.Company.Product.ClassName", actual);
        }
    }
}
