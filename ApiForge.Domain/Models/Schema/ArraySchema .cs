namespace ApiForge.Domain.Models.Schema
{
    /// <summary>
    /// Represents an array schema whose element structure is described by the ItemSchema property.
    /// </summary>
    public sealed record ArraySchema : ApiSchema
    {
        /// <summary>
        /// Gets the schema that describes the structure of the items in the array.
        /// </summary>
        public required ApiSchema ItemSchema { get; init; }
    }
}
