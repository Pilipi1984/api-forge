using System.Text.Json.Serialization;

namespace ApiForge.ApplicationCore.Enums
{
    /// <summary>
    /// Enum that contains the API status
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ApiStatus
    {
        /// <summary>
        /// The API is OK
        /// </summary>
        OK,
        /// <summary>
        /// The API has problems
        /// </summary>
        NoOk
    }
}
