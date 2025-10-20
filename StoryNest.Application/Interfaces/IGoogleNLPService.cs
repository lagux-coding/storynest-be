using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface IGoogleNLPService
    {
        Task<(float Score, float Magnitude)> AnalyzeSentimentAsync(string text);
        Task<(float Score, float Magnitude)> AnalyzeSentimentV2Async(string title, string content);
    }
}
