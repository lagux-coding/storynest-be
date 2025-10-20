using Elastic.Clients.Elasticsearch.Nodes;
using Google.Cloud.Language.V1;
using Microsoft.Extensions.Configuration;
using StoryNest.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Services.Google.GoogleNLP
{
    public class GoogleNLPService : IGoogleNLPService
    {
        private readonly LanguageServiceClient _googleClient;
        private readonly IConfiguration _config;

        public GoogleNLPService(IConfiguration config)
        {
            Console.WriteLine(config["GOOGLE_APPLICATION_CREDENTIALS"]);
            _googleClient = LanguageServiceClient.Create();
            _config = config;
        }

        public async Task<(float Score, float Magnitude)> AnalyzeSentimentAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return (0, 0);

            var document = new Document
            {
                Content = text,
                Type = Document.Types.Type.PlainText
            };

            var response = await _googleClient.AnalyzeSentimentAsync(document);
            var sentiment = response.DocumentSentiment;

            if (sentiment == null)
                return (0, 0);

            return (sentiment.Score, sentiment.Magnitude);
        }

        public async Task<(float Score, float Magnitude)> AnalyzeSentimentV2Async(string title, string content)
        {
            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
                return (0, 0);

            var text = $"{title?.Trim()}\n\n{content?.Trim()}";

            var document = new Document
            {
                Content = text,
                Type = Document.Types.Type.PlainText,
                Language = "vi",
            };

            var response = await _googleClient.AnalyzeSentimentAsync(document);
            var sentiment = response.DocumentSentiment;

            if (sentiment == null)
                return (0, 0);

            return (sentiment.Score, sentiment.Magnitude);
        }
    }
}
