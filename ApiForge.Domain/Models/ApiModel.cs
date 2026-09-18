namespace ApiForge.Domain.Models
{
    /// <summary>
    /// Model of APi
    /// </summary>
    public class ApiModel
    {
        /// <summary>
        /// Name of model
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// List of Api properties
        /// </summary>
        public List<ApiProperty> Properties { get; set; } = [];
    }
}
