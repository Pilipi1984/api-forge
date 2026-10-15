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
        /// Gets the OpenAPI type associated with the API schema.
        /// </summary>
        public required string OpenApiType { get; init; }
        /// <summary>
        /// Gets the .NET type associated with the API schema.
        /// </summary>
        public required string ClrType { get; init; }
        /// <summary>
        /// Gets a value indicating whether the API schema is nullable.
        /// </summary>
        public bool Nullable { get; init; }
        /// <summary>
        /// Gets the description of the API schema.
        /// </summary>
        public string? Description { get; init; }
    }
}
