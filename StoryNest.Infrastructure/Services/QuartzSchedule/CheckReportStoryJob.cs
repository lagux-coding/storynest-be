using Microsoft.Extensions.Logging;
using Quartz;
using StoryNest.Application.Interfaces;
using StoryNest.Domain.Enums;
using StoryNest.Shared.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Services.QuartzSchedule
{
    [DisallowConcurrentExecution]
    public class CheckReportStoryJob : IJob
    {
        private readonly ILogger<CheckReportStoryJob> _logger;
        private readonly IUserReportService _userReportService;
        private readonly IGoogleNLPService _googleNLPService;
        private readonly IStorySentimentAnalysisService _storySentimentAnalysisService;
        private readonly IStoryService _storyService;
        private readonly INotificationService _notificationService;

        public CheckReportStoryJob(ILogger<CheckReportStoryJob> logger, IUserReportService userReportService, IGoogleNLPService googleNLPService, IStorySentimentAnalysisService storySentimentAnalysisService, IStoryService storyService, INotificationService notificationService)
        {
            _logger = logger;
            _userReportService = userReportService;
            _googleNLPService = googleNLPService;
            _storySentimentAnalysisService = storySentimentAnalysisService;
            _storyService = storyService;
            _notificationService = notificationService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            try
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

                var runId = context.FireInstanceId;
                _logger.LogInformation($"[NLP Job] Start RunId={runId} at {DateTime.UtcNow}");

                var reports = await _userReportService.GetAllPendingReportsAsync();
                foreach (var report in reports)
                {
                    try
                    {
                        var story = report.ReportedStory!;
                        var normalized = TextNormalizer.NormalizeStoryText(story.Content);
                        var (score, magnitude) = await _googleNLPService.AnalyzeSentimentV2Async(story.Title, normalized);

                        var result = await _storySentimentAnalysisService.CreateStorySentimentAnalysisAsync(story.Id, score, magnitude, normalized, "GoogleNLP", runId);

                        if (result > 0)
                        {
                            _logger.LogInformation($"[NLP Job] Successfully processed report Id={report.ReportedStoryId}, StoryId={story.Id}, Score={score}, Magnitude={magnitude}");

                            // Optionally, update the report status to Resolved here
                            if (score < -0.25f && report.Status == ReportStatus.Pending)
                            {
                                report.Status = ReportStatus.Resolved;
                                report.UpdatedAt = DateTime.UtcNow;
                                var reportResult = await _userReportService.UpdateUserReportAsync(report);
                                if (reportResult > 0)
                                {
                                    story.PrivacyStatus = PrivacyStatus.Unlisted;
                                    story.StoryStatus = StoryStatus.Deleted;
                                    await _storyService.UpdateWithEntityAsync(story);

                                    // Notify user
                                    await _notificationService.SendNotificationAsync(story.UserId, content: $"Story của bạn '{story.Title}' đã bị xóa do có nhiều báo cáo và phân tích cảm xúc tiêu cực. ", type: NotificationType.System, referenceId: story.Id, referenceType: "story", actorId: null);
                                }
                                _logger.LogInformation($"[NLP Job] Report Id={report.ReportedStoryId} marked as Resolved due to negative sentiment.");
                            }
                        }
                        else
                        {
                            _logger.LogWarning($"[NLP Job] Failed to save sentiment analysis for report Id={report.ReportedStoryId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error processing report Id={report.ReportedStoryId}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing CheckReportStoryJob.");
            }
        }
    }
}
