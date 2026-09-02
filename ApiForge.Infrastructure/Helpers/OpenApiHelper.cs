using Microsoft.OpenApi;

namespace ApiForge.Infrastructure.Helpers
{
    internal static class OpenApiHelper
    {
        internal static bool GetNullable(IOpenApiSchema schema)
        {
            if (schema is null)
            {
                return false;
            }

            // If the schema type is explicitly set to null, it is considered nullable
            if (schema.Type == JsonSchemaType.Null)
            {
                return true;
            }

            // If the "nullable" keyword exists in UnrecognizedKeywords (OpenAPI v3.0)
            if (schema.UnrecognizedKeywords is not null && schema.UnrecognizedKeywords.TryGetValue("nullable", out var node) && node is not null)
            {
                var txt = node.ToString();
                if (bool.TryParse(txt, out var b))
                {
                    return b;
                }

                if (string.Equals(txt, "true", StringComparison.OrdinalIgnoreCase) || txt == "1")
                {
                    return true;
                }
            }

            return false;
        }
    }
}
