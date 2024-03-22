using System.Text.Json;
using API.Helpers;

namespace API.Extensions;

public static class HttpExtensions
{
    public static void AddPaginationHeader(this HttpResponse response, PaginationHeader header)
    {
        //Not in the context of controller > make it camel case
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        //Custom header 
        response.Headers.Add("Pagination", JsonSerializer.Serialize(header, jsonOptions));
        //explicitly allow cos header policy inside the method or client can't access Pagination header
        response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
    }
}