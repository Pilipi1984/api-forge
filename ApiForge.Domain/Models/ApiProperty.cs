namespace ApiForge.Domain.Models
{
    /// <summary>
    /// API Property
    /// </summary>
    public class ApiProperty
    {
        /// <summary>
        /// Name ot the property
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Type
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Is required
        /// </summary>
        public bool Required { get; set; }

        /// <summary>
        /// Is nullable
        /// </summary>
        public bool Nullable { get; set; }

        /// <summary>
        /// Format
        /// </summary>
        public string? Format { get; set; }

        /// <summary>
        /// Description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Default value
        /// </summary>
        public object? DefaultValue { get; set; }
    }
}
