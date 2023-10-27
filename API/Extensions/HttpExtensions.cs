using System.Text.Json;
using API.Helpers;

namespace API.Extensions;

public static class HttpExtensions
{
    public static void AddPaginationHeader(this HttpResponse response, PaginationHeader header)
    {
        var jsonOptions = new JsonSerializerOptions{PropertyNamingPolicy = JsonNamingPolicy.CamelCase};
        response.Headers.Add("Pagination", JsonSerializer.Serialize(header, jsonOptions));
        
        // Since this is a custom header, explicitly allow CORS policy.
        response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
    }
}