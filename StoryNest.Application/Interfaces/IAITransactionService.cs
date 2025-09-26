using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface IAITransactionService
    {
        public Task<int> AddTransactionAsync(long userId, int referenceId, int amount, string desc, AITransactionType type);
    }
}
