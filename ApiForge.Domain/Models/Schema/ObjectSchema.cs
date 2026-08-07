namespace ApiForge.Domain.Models.Schema
{
    public sealed record ObjectSchema : ApiSchema
    {
        public IReadOnlyList<ApiProperty> Properties { get; init; }
            = [];
    }
}
