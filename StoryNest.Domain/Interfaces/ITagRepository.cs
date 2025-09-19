using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Interfaces
{
    public interface ITagRepository
    {
        public Task<Tag?> GetByNameAsync(string name);
        public Task AddAsync(Tag tag);
        public Task<int> GetIdByNameAsync(string name);
    }
}
