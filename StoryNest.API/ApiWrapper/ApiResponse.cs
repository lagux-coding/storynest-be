using System.Text.Json.Serialization;

namespace StoryNest.API.ApiWrapper
{
    public class ApiResponse<T> where T : class
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }
        [JsonPropertyName("message")]
        public string? Message { get; set; }
        [JsonPropertyName("data")]
        public T? Data { get; set; }

        public ApiResponse()
        {
        }

        public ApiResponse(T? data, string? message, int? statusCode = StatusCodes.Status200OK)
        {
            Status = statusCode!.Value;
            Message = message;
            Data = data;
        }

        // Helper methods
        public static ApiResponse<T> Success(T data, string message = "success", int statusCode = StatusCodes.Status200OK) => new(data, message, statusCode);

        public static ApiResponse<T> Fail(T? data, string message = "fail", int statusCode = StatusCodes.Status400BadRequest) => new(data, message, statusCode);

        public static ApiResponse<T> NotFound(string message = "not found") => new(default, message, StatusCodes.Status404NotFound);

        public static ApiResponse<T> Error(string message, int statusCode = StatusCodes.Status500InternalServerError) => new(default, message, statusCode);

        public static ApiResponse<T> Forbbiden(string message = "forbbiden") => new(default, message, StatusCodes.Status403Forbidden);

    }
}
