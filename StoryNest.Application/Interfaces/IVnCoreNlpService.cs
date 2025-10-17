using StoryNest.Application.Dtos.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface IVnCoreNlpService
    {
        Task<List<List<TokenDto>>> AnalyzeTextAsync(string text);
        Task<List<TokenDto>> CompareOffensiveAsync(List<List<TokenDto>> tokenList);
    }
}
