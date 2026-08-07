using ApiForge.Domain.Models.ApiParameters.Enums;

namespace ApiForge.Domain.Models.ApiParameters
{
    public sealed class ApiHeaderParameter : ApiParameter
    {
        public ApiHeaderParameter()
        {
            Location = ParameterLocation.Header;
        }
    }
}
