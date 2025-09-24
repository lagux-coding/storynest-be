using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Interfaces
{
    public interface IUserMediaRepository
    {
        public Task AddAsync(UserMedia media);
        public Task UpdateAsync(UserMedia media);
        public Task<bool> ExistsAsync(Expression<Func<UserMedia, bool>> predicate);
    }
}
