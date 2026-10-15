using Microsoft.OpenApi;

namespace ApiForge.Domain.Models.ApiParameters
{
    /// <summary>
    /// Class for Api parameters type of Query
    /// </summary>
    public sealed class ApiQueryParameter : ApiParameter
    {
        /// <summary>
        /// Class for Api parameters type of Query
        /// </summary>
        public ApiQueryParameter() 
        {
            Location = ParameterLocation.Query;
        }

        /// <summary>
        /// Indicates if the values should be exploded into separate entries when serialized or represented.
        /// </summary>
        public bool Explode { get; init; }
    }
}
