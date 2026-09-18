using ApiForge.Domain.Models.ApiParameters;
using ApiForge.Domain.Models.Schema;

namespace ApiForge.Domain.Models
{
    /// <summary>
    /// API endpoint
    /// </summary>
    public class ApiEndpoint
    {
        /// <summary>
        /// Route
        /// </summary>
        public string Route { get; set; } = string.Empty;

        /// <summary>
        /// Type of httpMethod
        /// </summary>
        public string HttpMethod { get; set; } = string.Empty;

        public string OperationId { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;

        public List<ApiParameter> Parameters { get; set; } = [];

        public ApiSchema? RequestBody { get; set; }

        public ApiSchema? Response { get; set; }
    }
}
