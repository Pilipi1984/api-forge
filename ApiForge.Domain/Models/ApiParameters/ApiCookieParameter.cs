using Microsoft.OpenApi;

namespace ApiForge.Domain.Models.ApiParameters
{
    public sealed class ApiCookieParameter : ApiParameter
    {
        public ApiCookieParameter() 
        {
            Location = ParameterLocation.Cookie;
        }
    }
}
