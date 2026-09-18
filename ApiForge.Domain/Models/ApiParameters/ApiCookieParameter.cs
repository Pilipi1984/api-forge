using Microsoft.OpenApi;

namespace ApiForge.Domain.Models.ApiParameters
{
    /// <summary>
    /// Class for Api parameters type of Cookie
    /// </summary>
    public sealed class ApiCookieParameter : ApiParameter
    {
        public ApiCookieParameter() 
        {
            Location = ParameterLocation.Cookie;
        }
    }
}
