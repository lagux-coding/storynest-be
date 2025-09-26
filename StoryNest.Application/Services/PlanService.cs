using AutoMapper;
using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Interfaces;
using StoryNest.Domain.Entities;
using StoryNest.Domain.Interfaces;
using StoryNest.Shared.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Services
{
    public class PlanService : IPlanService
    {
        private readonly IPlanRepository _planRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PlanService(IPlanRepository planRepository, IMapper mapper, IUnitOfWork unitOfWork)
        {
            _planRepository = planRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> AddPlanAsync(CreatePlanRequest request)
        {
            try
            {
                var plan = _mapper.Map<Plan>(request);
                plan.Slug = SlugGenerationHelper.GenerateSlug(request.Name);
                plan.CreatedAt = DateTime.UtcNow;
                await _planRepository.AddAsync(plan);
                return await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
