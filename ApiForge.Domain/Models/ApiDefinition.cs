using ApiForge.Domain.Enums;

namespace ApiForge.Domain.Models
{
    /// <summary>
    /// Represents the definition of an API, including its title, version, architecture style, endpoints, and models.
    /// </summary>
    public class ApiDefinition
    {
        /// <summary>
        /// Gets or sets the title of the API.
        /// </summary>
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the version of the API.
        /// </summary>
        public string Version { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the architecture style of the API.
        /// </summary>
        public ArchitectureStyle Architecture { get; set; } = ArchitectureStyle.Clean;
        /// <summary>
        /// Gets or sets the list of endpoints for the API.
        /// </summary>
        public List<ApiEndpoint> Endpoints { get; set; } = [];
        /// <summary>
        /// Gets or sets the list of models for the API.
        /// </summary>
        public List<ApiModel> Models { get; set; } = [];
    }
}
