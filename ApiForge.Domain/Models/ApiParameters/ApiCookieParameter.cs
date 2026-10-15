using Microsoft.OpenApi;

namespace ApiForge.Domain.Models.ApiParameters
{
    /// <summary>
    /// Class for Api parameters type of Cookie
    /// </summary>
    public sealed class ApiCookieParameter : ApiParameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiCookieParameter"/> class.
        /// </summary>
        public ApiCookieParameter() 
        {
            Location = ParameterLocation.Cookie;
        }
    }
}
