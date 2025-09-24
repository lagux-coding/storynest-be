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
    }
}
