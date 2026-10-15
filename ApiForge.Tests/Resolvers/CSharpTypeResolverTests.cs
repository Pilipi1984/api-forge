using ApiForge.Domain.Models;
using ApiForge.Infrastructure.Generator.Resolvers;

namespace ApiForge.Tests.Resolvers
{
    /// <summary>
    /// Tests for the <see cref="CSharpTypeResolver"/> class.
    /// </summary>
    public class CSharpTypeResolverTests
    {
        /// <summary>
        /// Tests that the NormalizePrimitive method correctly maps CLR types to C# primitive types.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="expected"></param>
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

        /// <summary>
        /// Tests that the ResolveProperty method correctly appends a question mark for nullable properties.
        /// </summary>
        [Fact]
        public void ResolveProperty_Nullable_AppendsQuestion()
        {
            var prop = new ApiProperty { Name = "Id", Type = "Int32", Nullable = true };
            var type = CSharpTypeResolver.ResolveProperty(prop);
            Assert.Equal("int?", type);
        }

        /// <summary>
        /// Tests that the Resolve method correctly resolves an array schema to a List of the appropriate type.
        /// </summary>
        [Fact]
        public void Resolve_Array_ProducesList()
        {
            var array = new Domain.Models.Schema.ArraySchema { OpenApiType = "array", ClrType = "List", ItemSchema = new Domain.Models.Schema.PrimitiveSchema { OpenApiType = "integer", ClrType = "Int32" } };
            var actual = CSharpTypeResolver.Resolve(array, "Root.Models");
            Assert.Equal("List<int>", actual);
        }

        /// <summary>
        /// Tests that the Resolve method correctly resolves an enum schema to a string type 
        /// when the OpenApiType is "string" and the schema is nullable.
        /// </summary>
        [Fact]
        public void Resolve_Enum_IsString_WithNullable()
        {
            var e = new Domain.Models.Schema.EnumSchema { OpenApiType = "string", ClrType = "String", Nullable = true };
            var actual = CSharpTypeResolver.Resolve(e, "Root.Models");
            Assert.Equal("string?", actual);
        }

        /// <summary>
        /// Tests that the Resolve method correctly resolves a reference schema to its full name within the specified models namespace.
        /// </summary>
        [Fact]
        public void Resolve_Reference_ResolvesFullName()
        {
            var r = new Domain.Models.Schema.ReferenceSchema { OpenApiType = "ref", ClrType = "Ref", ReferenceName = "Company.Product.ClassName" };
            var actual = CSharpTypeResolver.Resolve(r, "Root.Models");
            Assert.Equal("Root.Models.Company.Product.ClassName", actual);
        }
    }
}
