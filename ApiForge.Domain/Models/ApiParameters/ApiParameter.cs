using Microsoft.OpenApi;

namespace ApiForge.Domain.Models.ApiParameters
{
    /// <summary>
    /// Class for Api parameter
    /// </summary>
    public abstract class ApiParameter
    {
        /// <summary>
        /// Name of parameter
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Type of parameter
        /// </summary>
        public required string Type { get; init; }

        /// <summary>
        /// Parameter location
        /// </summary>
        public required ParameterLocation Location { get; init; }

        /// <summary>
        /// Is required
        /// </summary>
        public bool Required { get; init; }

        /// <summary>
        /// Parameter description
        /// </summary>
        public string? Description { get; init; }
    }
}
