namespace ApiForge.Domain.Models.Schema
{
    public sealed record ArraySchema : ApiSchema
    {
        public required ApiSchema ItemSchema { get; init; }
    }
}
