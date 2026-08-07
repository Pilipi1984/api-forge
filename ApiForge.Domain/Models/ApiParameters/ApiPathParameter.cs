using ApiForge.Domain.Models.ApiParameters.Enums;

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
