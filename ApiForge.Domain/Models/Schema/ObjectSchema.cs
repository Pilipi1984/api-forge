namespace ApiForge.Domain.Models.Schema
{
    /// <summary>
    /// Represents an object schema with a set of properties.
    /// </summary>
    public sealed record ObjectSchema : ApiSchema
    {
        public IReadOnlyList<ApiProperty> Properties { get; init; }
            = [];
    }
}
