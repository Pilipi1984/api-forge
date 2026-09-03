using ApiForge.Domain.Models;
using ApiForge.Domain.Models.Schema;
using ApiForge.Infrastructure.Helpers;

namespace ApiForge.Infrastructure.Generator.Resolvers
{
    /// <summary>
    /// Provides methods to resolve C# types from API schema definitions, 
    /// including handling of references, arrays, enums, objects, and primitive types, while considering nullability.
    /// </summary>
    public static class CSharpTypeResolver
    {
        /// <summary>
        /// Resolves the C# type for a given API schema, taking into account the schema type, nullability, and the specified models namespace.
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="modelsNamespace"></param>
        /// <returns>Returns the resolved C# type.</returns>
        public static string Resolve(ApiSchema? schema, string modelsNamespace)
        {
            if (schema is null)
                return "object";

            var baseType = schema switch
            {
                ReferenceSchema reference => ResolveReferenceType(reference, modelsNamespace),
                ArraySchema array => $"List<{Resolve(array.ItemSchema, modelsNamespace)}>",
                EnumSchema => "string",
                ObjectSchema => "object",
                PrimitiveSchema primitive => NormalizePrimitive(primitive.ClrType),
                _ => "object"
            };

            return schema.Nullable ? baseType + "?" : baseType;
        }

        /// <summary>
        /// Resolves the C# type for a reference schema by determining the fully qualified name based on the reference name and the provided models namespace.
        /// </summary>
        /// <param name="reference"></param>
        /// <param name="modelsNamespace"></param>
        /// <returns>Returns the resolved C# type.</returns>
        private static string ResolveReferenceType(ReferenceSchema reference, string modelsNamespace)
        {
            var resolved = QualifiedNameResolver.Resolve(reference.ReferenceName);
            var fullNamespace = QualifiedNameResolver.BuildNamespace(modelsNamespace, resolved.NamespaceSegments);

            return $"{fullNamespace}.{resolved.ClassName}";
        }

        /// <summary>
        /// Resolves the C# type for a given API property, taking into account the property's type and nullability.
        /// </summary>
        /// <param name="property"></param>
        /// <returns>Returns the resolved C# type.</returns>
        public static string ResolveProperty(ApiProperty property)
        {
            var baseType = NormalizePrimitive(property.Type);
            return property.Nullable ? baseType + "?" : baseType;
        }

        /// <summary>
        /// Normalizes the primitive type to its C# equivalent.
        /// </summary>
        /// <param name="clrType"></param>
        /// <returns>Returns the normalized C# type.</returns>
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