namespace ApiForge.Domain.Models.Schema
{
    /// <summary>
    /// Represents an enum schema with a set of allowed values.
    /// </summary>
    public sealed record EnumSchema : ApiSchema
    {
        public IReadOnlyList<string> Values { get; init; }
            = [];
    }
}
