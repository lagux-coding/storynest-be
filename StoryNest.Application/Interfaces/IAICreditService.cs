using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface IAICreditService
    {
        public Task AddCreditsAsync(long userId, int amount);
        public Task UpdateCreditsAsync(AICredit aIiCredit);
        public Task<AICredit?> GetUserCredit(long userId);
    }
}
