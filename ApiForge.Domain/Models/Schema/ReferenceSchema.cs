namespace ApiForge.Domain.Models.Schema
{
    public sealed record ReferenceSchema : ApiSchema
    {
        public required string ReferenceName { get; init; }
    }
}
