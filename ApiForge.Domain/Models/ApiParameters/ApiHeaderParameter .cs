using Microsoft.OpenApi;

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
