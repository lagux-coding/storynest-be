using Microsoft.Extensions.Logging;
using Quartz;
using StoryNest.Application.Interfaces;
using StoryNest.Application.Services;
using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Services.QuartzSchedule
{
    [DisallowConcurrentExecution]
    public class RenewCreditJob : IJob
    {
        private readonly ILogger<RenewCreditJob> _logger;
        private readonly IUserService _userService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IAICreditService _aiCreditService;
        private readonly IAITransactionService _aiTransactionService;

        public RenewCreditJob(ILogger<RenewCreditJob> logger, IUserService userService, ISubscriptionService subscriptionService, IAICreditService aiCreditService, IAITransactionService aiTransactionService)
        {
            _logger = logger;
            _userService = userService;
            _subscriptionService = subscriptionService;
            _aiCreditService = aiCreditService;
            _aiTransactionService = aiTransactionService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var nowUtc = DateTime.UtcNow;
            var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc,
                TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"));

            _logger.LogInformation("Job executed at UTC: {utc} | Local (Asia/Ho_Chi_Minh): {local}",
                nowUtc, nowLocal);
            if (context.Trigger is ICronTrigger cronTrigger)
            {
                var tz = cronTrigger.TimeZone;
                _logger.LogInformation("Cron trigger timezone: {tz}", tz.Id);
            }

            try
            {    
                var users = await _userService.GetAllUser();
                if (users == null || users.Count == 0)
                {
                    _logger.LogInformation("No users found for credit renewal.");
                    return;
                }

                foreach(var user in users)
                {
                    var sub = await _subscriptionService.GetActiveSubByUser(user.Id);
                    var credit = await _aiCreditService.GetUserCredit(user.Id);                    
                    AITransaction transaction;
                    if (sub == null)
                    {
                        _logger.LogInformation("User {userId} has no active subscription", user.Id);
                        credit.TotalCredits = 10;
                        transaction = new()
                        {
                            UserId = user.Id,
                            Type = Domain.Enums.AITransactionType.Earned,
                            Amount = 10,
                            Description = "renew credit daily (default no-sub plan)",
                            BalanceAfter = credit.TotalCredits,
                            ReferenceId = credit.Id,
                            CreatedAt = DateTime.UtcNow
                        };

                        await _aiCreditService.UpdateCreditsAsync(credit);
                        await _aiTransactionService.AddTransactionByEntityAsync(transaction);
                        continue;
                    }

                    if (sub.Plan == null)
                    {
                        _logger.LogWarning("User {UserId} subscription {SubId} has no Plan loaded.", user.Id, sub.Id);
                        continue;
                    }

                    var plan = sub.Plan;
                    credit.TotalCredits = plan.AiCreditsDaily;
                    transaction = new()
                    {
                        UserId = user.Id,
                        Type = Domain.Enums.AITransactionType.Earned,
                        Amount = plan.AiCreditsDaily,
                        Description = $"renew credit daily ({plan.Name} plan)",
                        BalanceAfter = credit.TotalCredits,
                        ReferenceId = credit.Id,
                        CreatedAt = DateTime.UtcNow
                    };

                    await _aiCreditService.UpdateCreditsAsync(credit);
                    await _aiTransactionService.AddTransactionByEntityAsync(transaction);
                    continue;
                }

                _logger.LogInformation("Renew credits completed at {time}", DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RenewCreditJob");
            }
        }
    }
}
