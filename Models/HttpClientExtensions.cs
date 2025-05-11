using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Ebertin.Models
{
    public static class HttpClientExtensions
    {
        public static async Task<T> PostAsJsonAsync<T>(this HttpClient client, string requestUri, HttpContent content)
        {
            var response = await client.PostAsync(requestUri, content);
            
            var jsonContent = await response.Content.ReadAsStringAsync();
            
            if (response.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<T>(jsonContent);
            }
            else
            {
                try
                {
                    using var doc = JsonDocument.Parse(jsonContent);
                    var error = doc.RootElement.GetProperty("error").GetString();
                    throw new HttpRequestException($"Error {response.StatusCode}: {error}");
                }
                catch (JsonException)
                {
                    // If we can't parse the error, just return the raw content
                    throw new HttpRequestException($"Error {response.StatusCode}: {jsonContent}");
                }
            }
        }
    }
}