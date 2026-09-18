namespace ApiForge.Domain.Models.Schema
{

    /// <summary>
    /// Represents a reference schema that points to another schema definition.
    /// </summary>
    public sealed record ReferenceSchema : ApiSchema
    {
        public required string ReferenceName { get; init; }
    }
}
