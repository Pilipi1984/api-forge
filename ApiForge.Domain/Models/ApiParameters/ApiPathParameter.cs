using Microsoft.OpenApi;

namespace ApiForge.Domain.Models.ApiParameters
{
    /// <summary>
    /// Class for Api parameters type of Path
    /// </summary>
    public sealed class ApiPathParameter : ApiParameter
    {
        public ApiPathParameter()
        {
            Location = ParameterLocation.Path;
        }
    }
}
