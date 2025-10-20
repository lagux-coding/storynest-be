using Google.Cloud.Language.V1;
using Microsoft.Extensions.Configuration;
using StoryNest.Application.Dtos.Dto;
using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Interfaces;
using StoryNest.Shared.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
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

        public async Task<List<List<TokenDto>>> AnalyzeTextAsync(string text)
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

                var json = await response.Content.ReadAsStringAsync();
                var parsed = JsonSerializer.Deserialize<Root>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var allSentences = new List<List<TokenDto>>();
                if (parsed?.Data != null)
                {
                    foreach (var kv in parsed.Data)
                    {
                        allSentences.Add(kv.Value);
                    }
                }

                Console.WriteLine($"[VNCoreNLP] Saved analyzed result. Total: {allSentences.Count}");

                return allSentences;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<TokenDto>> CompareOffensiveAsync(CheckOffensiveRequest request)
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "data", "vn_offensive_words.txt");
                var offensiveWords = OffensiveWordLoader.LoadWords(filePath);
                var found = new List<TokenDto>();

                var tokenList = await this.AnalyzeTextAsync(request.Title);
                tokenList.AddRange(await this.AnalyzeTextAsync(request.Content));

                // Analyze tags if any
                foreach (var tag in request.Tags)
                {
                    var tagTokens = await this.AnalyzeTextAsync(tag);
                    tokenList.AddRange(tagTokens);
                }

                foreach (var sentence in tokenList)
                {
                    foreach (var token in sentence)
                    {
                        var word = token.WordForm.Replace("_", " ").ToLowerInvariant();
                        if (offensiveWords.Contains(word))
                        {
                            found.Add(token);
                        }
                    }

                    var joinedSentence = string.Join(" ", sentence.Select(t => t.WordForm.Replace("_", " ").ToLowerInvariant()));
                    foreach (var bad in offensiveWords)
                    {
                        if (joinedSentence.Contains(bad) && bad.Contains(" "))
                        {
                            // nếu cụm có chứa khoảng trắng, tìm token trùng 1 phần của cụm
                            foreach (var token in sentence)
                            {
                                if (bad.Contains(token.WordForm.ToLowerInvariant()))
                                {
                                    found.Add(token);
                                }
                            }
                        }
                    }
                }

                var distinct = found
                    .GroupBy(f => f.Index)
                    .Select(g => g.First())
                    .ToList();

                return await Task.FromResult(distinct);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
