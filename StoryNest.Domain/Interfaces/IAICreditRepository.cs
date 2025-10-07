using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Interfaces
{
    public interface IAICreditRepository
    {
        public Task AddAsync(AICredit credit);
        public void UpdateAsync(AICredit credit);
        public Task<AICredit?> GetById(int id);
        public Task<AICredit?> GetByUserId(long userId);
    }
}
