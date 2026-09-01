using Microsoft.OpenApi;

namespace ApiForge.Domain.Models.ApiParameters
{
    public sealed class ApiPathParameter : ApiParameter
    {
        public ApiPathParameter()
        {
            Location = ParameterLocation.Path;
        }
    }
}
