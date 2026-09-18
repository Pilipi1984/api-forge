namespace ApiForge.Domain.Models.Schema
{
    /// <summary>
    /// Record to store the API Schema
    /// </summary>
    public abstract record ApiSchema
    {
        /// <summary>
        /// Name of Schema
        /// </summary>
        public string Name { get; init; } = string.Empty;
        /// <summary>
        /// 
        /// </summary>
        public required string OpenApiType { get; init; }

        public required string ClrType { get; init; }

        public bool Nullable { get; init; }

        public string? Description { get; init; }
    }
}
