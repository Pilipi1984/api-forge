namespace ApiForge.Domain.Models.Schema
{
    /// <summary>
    /// Represents an enum schema with a set of allowed values.
    /// </summary>
    public sealed record EnumSchema : ApiSchema
    {
        /// <summary>
        /// Gets the list of allowed values for the enum schema.
        /// </summary>
        public IReadOnlyList<string> Values { get; init; } = [];
    }
}
