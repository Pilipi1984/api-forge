namespace ApiForge.Domain.Models.Schema
{

    /// <summary>
    /// Represents a reference schema that points to another schema definition.
    /// </summary>
    public sealed record ReferenceSchema : ApiSchema
    {
        /// <summary>
        /// Gets the name of the referenced schema.
        /// </summary>
        public required string ReferenceName { get; init; }
    }
}
