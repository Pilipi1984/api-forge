using Microsoft.OpenApi;

namespace ApiForge.Domain.Models.ApiParameters
{
    /// <summary>
    /// Class for Api parameters type of Header
    /// </summary>
    public sealed class ApiHeaderParameter : ApiParameter
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiHeaderParameter"/> class.
        /// </summary>
        public ApiHeaderParameter()
        {
            Location = ParameterLocation.Header;
        }
    }
}
