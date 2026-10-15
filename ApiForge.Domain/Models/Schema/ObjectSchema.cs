namespace ApiForge.Domain.Models.Schema
{
    /// <summary>
    /// Represents an object schema with a set of properties.
    /// </summary>
    public sealed record ObjectSchema : ApiSchema
    {
        /// <summary>
        /// Gets the list of properties for the object schema.
        /// </summary>
        public IReadOnlyList<ApiProperty> Properties { get; init; } = [];
    }
}
