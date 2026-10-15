using Microsoft.OpenApi;

namespace ApiForge.Domain.Models.ApiParameters
{
    /// <summary>
    /// Class for Api parameters type of Path
    /// </summary>
    public sealed class ApiPathParameter : ApiParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiPathParameter"/> class.
        /// </summary>
        public ApiPathParameter()
        {
            Location = ParameterLocation.Path;
        }
    }
}
