namespace ApiForge.Domain.Models
{
    public class ApiDefinition
    {
        public string Title { get; set; } = string.Empty;

        public string Version { get; set; } = string.Empty;

        public List<ApiEndpoint> Endpoints { get; set; } = [];

        public List<ApiModel> Models { get; set; } = [];
    }
}
