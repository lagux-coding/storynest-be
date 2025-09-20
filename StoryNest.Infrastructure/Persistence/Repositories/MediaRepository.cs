using Microsoft.EntityFrameworkCore;
using StoryNest.Domain.Entities;
using StoryNest.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Persistence.Repositories
{
    public class MediaRepository : IMediaRepository 
    {
        private readonly MyDbContext _context;

        public MediaRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task CreateMediaAsync(Media media)
        {
            await _context.Media.AddAsync(media);
        }

        public async Task DeleteMediaByStoryId(int storyId)
        {
            await _context.Media
                .Where(m => m.StoryId == storyId)
                .ForEachAsync(m => _context.Media.Remove(m));
        }
    }
}
