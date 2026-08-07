namespace ApiForge.Domain.Models
{
    public class ApiProperty
    {
        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public bool Required { get; set; }

        public bool Nullable { get; set; }

        public string? Format { get; set; }

        public string? Description { get; set; }

        public object? DefaultValue { get; set; }
    }
}
