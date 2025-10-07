using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Interfaces
{
    public interface IAIUsageLogRepository
    {
        public Task AddAsync(AIUsageLog usage);
    }
}
