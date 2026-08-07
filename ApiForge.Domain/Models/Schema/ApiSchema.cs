namespace ApiForge.Domain.Models.Schema
{
    public abstract record ApiSchema
    {
        public string Name { get; init; } = string.Empty;
        public required string OpenApiType { get; init; }

        public required string ClrType { get; init; }

        public bool Nullable { get; init; }

        public string? Description { get; init; }
    }
}
