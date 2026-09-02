using ApiForge.Domain.Models;
using ApiForge.Domain.Models.Schema;
using ApiForge.Infrastructure.Helpers;

namespace ApiForge.Infrastructure.Generator.Resolvers
{
    /// <summary>
    /// Resolves C# types from API schema definitions.
    /// </summary>
    public static class CSharpTypeResolver
    {
        private const string Object = "object";

        /// <summary>
        /// Resolves the C# type for a given API schema, considering its nullability and the provided models namespace.
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="modelsNamespace"></param>
        /// <returns>The resolved C# type.</returns>
        public static string Resolve(ApiSchema? schema, string modelsNamespace)
        {
            if (schema is null)
                return Object;

            var baseType = schema switch
            {
                ReferenceSchema reference => $"{modelsNamespace}.{NameHelper.ToPascalCase(reference.ReferenceName)}",
                ArraySchema array => $"List<{Resolve(array.ItemSchema, modelsNamespace)}>",
                EnumSchema => "string",
                ObjectSchema => Object,
                PrimitiveSchema primitive => NormalizePrimitive(primitive.ClrType),
                _ => Object
            };

            return schema.Nullable ? baseType + "?" : baseType;
        }

        public static string ResolveProperty(ApiProperty property)
        {
            var baseType = NormalizePrimitive(property.Type);
            return property.Nullable ? baseType + "?" : baseType;
        }

        public static string NormalizePrimitive(string? clrType)
        {
            if (string.IsNullOrWhiteSpace(clrType))
                return Object;

            return clrType switch
            {
                "String" => "string",
                "Boolean" => "bool",
                "Int32" => "int",
                "Int64" => "long",
                "Double" => "double",
                "Single" => "float",
                _ => clrType
            };
        }
    }
}
