using StoryNest.Application.Dtos.Dto;
using StoryNest.Application.Dtos.Response;
using StoryNest.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUserService _userService;
        private readonly IStoryService _storyService;
        private readonly ICommentService _commentService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IPaymentService _paymentService;

        public DashboardService(IUserService userService, IStoryService storyService, ICommentService commentService, ISubscriptionService subscriptionService, IPaymentService paymentService)
        {
            _userService = userService;
            _storyService = storyService;
            _commentService = commentService;
            _subscriptionService = subscriptionService;
            _paymentService = paymentService;
        }

        public async Task<DashboardDto> GetDashboardStatsAsync()
        {
            try
            {
                var totalUsers = await _userService.GetAllUser();
                var totalStories = await _storyService.TotalStoriesAsync();
                var totalComments = await _commentService.TotalCommentsAsync();
                var totalSubscriptions = await _subscriptionService.TotalSubscriptionsAsync();

                return new DashboardDto
                {
                    TotalUsers = totalUsers.Count,
                    TotalStories = totalStories,
                    TotalComments = totalComments,
                    TotalSubscriptions = totalSubscriptions
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<DashboardStoriesResponse> GetDashboardStoryAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                var stories = await _storyService.GetAllStoriesAsync(page, pageSize);
                return new DashboardStoriesResponse
                {
                    Stories = stories,
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<SubscriptionDto> GetSubscriptionStatsAsync()
        {
            try
            {
                var totalUsers = await _userService.GetAllUser();
                var premiumUsers = await _subscriptionService.TotalPremiumUsersAsync();
                var freemiumUsers = totalUsers.Count - premiumUsers;

                var totalSubscriptions = await _subscriptionService.TotalSubscriptionsAsync();
                var activeSubscriptions = await _subscriptionService.TotalActiveSubscriptionsAsync();
                var totalRevenue = await _paymentService.TotalRevenueAsync();

                var bloomPlan = await _subscriptionService.PlanCountAsync(2);
                var flourishPlan = await _subscriptionService.PlanCountAsync(3);
                var ensemblePlan = await _subscriptionService.PlanCountAsync(4);

                PlanCountDto bloomDto = new PlanCountDto
                {
                    PlanName = bloomPlan.FirstOrDefault()?.Plan.Name,
                    UserCount = bloomPlan.Count
                };

                PlanCountDto flourishDto = new PlanCountDto
                {
                    PlanName = flourishPlan.FirstOrDefault()?.Plan.Name,
                    UserCount = flourishPlan.Count
                };

                PlanCountDto ensembleDto = new PlanCountDto
                {
                    PlanName = ensemblePlan.FirstOrDefault()?.Plan.Name,
                    UserCount = ensemblePlan.Count
                };

                return new SubscriptionDto
                {
                    TotalSubscriptions = totalSubscriptions,
                    ActiveSubscriptions = activeSubscriptions,
                    TotalRevenue = totalRevenue,
                    TotalFreemiumUsers = freemiumUsers,
                    TotalPremiumUsers = premiumUsers,
                    BloomPlan = bloomDto,
                    FlourishPlan = flourishDto,
                    EnsemblePlan = ensembleDto
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
