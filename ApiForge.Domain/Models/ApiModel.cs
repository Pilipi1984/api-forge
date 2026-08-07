namespace ApiForge.Domain.Models
{
    public class ApiModel
    {
        public string Name { get; set; } = string.Empty;

        public List<ApiProperty> Properties { get; set; } = [];
    }
}
