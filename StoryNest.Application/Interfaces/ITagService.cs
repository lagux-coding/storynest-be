using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface ITagService
    {
        public Task<Tag> GetTagAsync(string tagName);
        public Task CreateTagAsync(Tag tag);
    }
}
