namespace ApiForge.ApplicationCore.DTOs.Responses
{
    public sealed class StatusResponse
    {
        public required string Status { get; init; }
        public required string Version { get; init; }
        public required string Environment { get; init; }
        public required DateTime UtcTimestamp { get; init; }
        public required TimeSpan Uptime { get; init; }
    }
}
