namespace ApiForge.Domain.Models.Schema
{
    /// <summary>
    /// Represents an array schema whose element structure is described by the ItemSchema property.
    /// </summary>
    public sealed record ArraySchema : ApiSchema
    {
        public required ApiSchema ItemSchema { get; init; }
    }
}
