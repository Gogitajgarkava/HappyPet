namespace HappyPet.WebAPI.Models
{
    /// <summary>
    /// Unified error envelope returned by every API endpoint.
    /// </summary>
    public sealed class ApiErrorResponse
    {
        public string Code { get; init; } = "ERROR";

        public string Message { get; init; } = string.Empty;

        public string? TraceId { get; init; }

        public IDictionary<string, string[]>? Errors { get; init; }
    }
}
