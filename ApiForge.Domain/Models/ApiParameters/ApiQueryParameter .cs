using Microsoft.OpenApi;

namespace ApiForge.Domain.Models.ApiParameters
{
    public sealed class ApiQueryParameter : ApiParameter
    {
        public ApiQueryParameter() 
        {
            Location = ParameterLocation.Query;
        }

        public bool Explode { get; init; }
    }
}
