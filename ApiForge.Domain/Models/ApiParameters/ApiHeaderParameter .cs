using Microsoft.OpenApi;

namespace ApiForge.Domain.Models.ApiParameters
{
    /// <summary>
    /// Class for Api parameters type of Header
    /// </summary>
    public sealed class ApiHeaderParameter : ApiParameter
    {
        public ApiHeaderParameter()
        {
            Location = ParameterLocation.Header;
        }
    }
}
