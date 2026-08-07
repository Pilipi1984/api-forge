namespace ApiForge.Domain.Models.Schema
{
    public sealed record EnumSchema : ApiSchema
    {
        public IReadOnlyList<string> Values { get; init; }
            = [];
    }
}
