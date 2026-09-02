using ApiForge.Domain.Models;
using ApiForge.Domain.Models.Schema;
using ApiForge.Infrastructure.Helpers;

namespace ApiForge.Infrastructure.Generator.Resolvers
{
    public static class CSharpTypeResolver
    {
        public static string Resolve(ApiSchema? schema, string modelsNamespace)
        {
            if (schema is null)
                return "object";

            var baseType = schema switch
            {
                ReferenceSchema reference => $"{modelsNamespace}.{NameHelper.ToPascalCase(reference.ReferenceName)}",
                ArraySchema array => $"List<{Resolve(array.ItemSchema, modelsNamespace)}>",
                EnumSchema => "string",
                ObjectSchema => "object",
                PrimitiveSchema primitive => NormalizePrimitive(primitive.ClrType),
                _ => "object"
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
                return "object";

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
