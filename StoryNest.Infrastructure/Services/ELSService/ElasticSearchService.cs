using AutoMapper;
using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Configuration;
using StoryNest.Application.Dtos;
using StoryNest.Application.Dtos.Response;
using StoryNest.Application.Interfaces;
using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Services.ELSService
{
    public class ElasticSearchService : IELSService
    {
        private readonly ElasticsearchClient _client;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public ElasticSearchService(IConfiguration configuration, IMapper mapper)
        {
            _configuration = configuration;
            var uri = _configuration["ELS_URI"];
            var username = _configuration["ELS_USERNAME"];
            var password = _configuration["ELS_PASSWORD"];

            if (string.IsNullOrEmpty(uri))
                throw new Exception("env els not set!");

            var settings = new ElasticsearchClientSettings(new Uri(uri))
                .Authentication(new BasicAuthentication(username, password));

            // Create client
            _client = new ElasticsearchClient(settings);
            _mapper = mapper;
        }

        public async Task IndexStoryAsync(Story story)
        {
            var doc = _mapper.Map<ELSDoc>(story);
            var id = story.Id.ToString();

            var response = await _client.IndexAsync(doc, x => x.Index("stories_v2").Id(id));

            if (!response.IsValidResponse)
            {
                // Log chi tiết DebugInformation (request + response)
                Console.WriteLine("Index failed:");
                Console.WriteLine(response.DebugInformation);

                // Nếu muốn, throw exception kèm thông tin
                throw new Exception($"Elasticsearch indexing failed: {response.DebugInformation}");
            }
            else
            {
                Console.WriteLine($"Indexed story {id} successfully");
            }
        }

        public async Task UpdateStoryAsync(Story story)
        {
            var doc = _mapper.Map<ELSDoc>(story);
            var id = story.Id.ToString();

            var response = await _client.IndexAsync(doc, i => i
                .Index("stories_v2")
                .Id(id)
                .Refresh(Refresh.True) // để search ra ngay
            );

            if (!response.IsValidResponse)
            {
                // Log chi tiết DebugInformation (request + response)
                Console.WriteLine("Update failed:");
                Console.WriteLine(response.DebugInformation);

                // Nếu muốn, throw exception kèm thông tin
                throw new Exception($"Elasticsearch indexing failed: {response.DebugInformation}");
            }
            else
            {
                Console.WriteLine($"Update story {id} successfully");
            }
        }
    }
}
