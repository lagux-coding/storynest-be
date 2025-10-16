using Microsoft.Extensions.Configuration;
using StoryNest.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Services.VnCoreNlp
{
    public class VnCoreNlpService : IVnCoreNlpService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiUrl;
        private readonly string _apiKey;

        public VnCoreNlpService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiUrl = config["VNCORENLP_API_URL"] ?? throw new Exception("VNCORENLP_API_URL not found in config");
            _apiKey = config["VNCORENLP_API_KEY"] ?? throw new Exception("VNCORENLP_API_KEY not found in config");
        }

        public async Task<object> AnalyzeTextAsync(string text)
        {
            try
            {
                Console.WriteLine($"[VNCoreNLP] BaseAddress = {_httpClient.BaseAddress}");

                var payload = new { text };
                using var request = new HttpRequestMessage(HttpMethod.Post, _apiUrl);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
                request.Content = JsonContent.Create(payload);

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"VnCoreNLP API error ({response.StatusCode}): {error}");
                }

                return await response.Content.ReadFromJsonAsync<object>();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
