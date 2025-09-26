using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface IAIUsageLogService
    {
        public Task<int> AddUsageAsync(long userId, UsageFeature feature, int inputToken, int outputToken, int creditUsed);
    }
}
