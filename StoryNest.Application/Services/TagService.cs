using StoryNest.Application.Interfaces;
using StoryNest.Domain.Entities;
using StoryNest.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;
        private readonly IUnitOfWork _unitOfWork;

        public TagService(ITagRepository tagRepository, IUnitOfWork unitOfWork)
        {
            _tagRepository = tagRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task CreateTagAsync(Tag tag)
        {
            await _tagRepository.AddAsync(tag);
        }

        public async Task<List<Tag>> GetAllSystemTagsAsync()
        {
            return await _tagRepository.GetAllSystemTagAsync();
        }

        public async Task<Tag> GetTagAsync(string tagName)
        {
            try
            {
                return await _tagRepository.GetByNameAsync(tagName);
                
            }
            catch (Exception ex)
            {
                throw new Exception(tagName + " not found");
            }
        }

        public async Task<int> GetTagIdByNameAsync(string tagName)
        {
            return await _tagRepository.GetIdByNameAsync(tagName);
        }
    }
}
