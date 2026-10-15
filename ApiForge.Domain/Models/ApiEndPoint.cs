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

        /// <summary>
        /// The unique identifier for the operation associated with this API endpoint.
        /// </summary>
        public string OperationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the summary of the API endpoint, providing a brief description of its purpose and functionality.
        /// </summary>
        public string Summary { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the API endpoint, providing detailed information about its behavior, parameters, and expected responses.
        /// </summary>
        public List<ApiParameter> Parameters { get; set; } = [];

        /// <summary>
        /// Gets or sets the request body schema for the API endpoint, defining the structure and format of the data expected in the request payload.
        /// </summary>
        public ApiSchema? RequestBody { get; set; }

        /// <summary>
        /// Gets or sets the response schema for the API endpoint, defining the structure and format of the data returned in the response payload.
        /// </summary>
        public ApiSchema? Response { get; set; }
    }
}
