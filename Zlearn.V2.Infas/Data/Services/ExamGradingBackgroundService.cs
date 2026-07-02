using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Common.Utils;
using Zlearn.V2.Application.Exams.DTOs;
using Zlearn.V2.Infas.External.SignalR;
using Zlearn.V2.Domain.ExamContext.Participants;
using Zlearn.V2.Infas.Data;

namespace Zlearn.V2.Infas.Data.Services
{
    public class ExamGradingBackgroundService : BackgroundService
    {
        private readonly ILogger<ExamGradingBackgroundService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IRedisService _redisService;
        private Timer? _timer;

        public ExamGradingBackgroundService(
            ILogger<ExamGradingBackgroundService> logger,
            IServiceProvider serviceProvider,
            IRedisService redisService)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _redisService = redisService;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("V2 Exam grading background service is starting.");
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }

        private async void DoWork(object? state)
        {
            try
            {
                var tasksJson = await _redisService.ListPopAll(RedisKeys.EXAM_GRADING_QUEUE, "global");
                if (tasksJson == null || tasksJson.Count == 0)
                {
                    return;
                }

                _logger.LogInformation("V2: Retrieved {count} grading tasks from queue.", tasksJson.Count);

                var tasks = tasksJson
                    .Select(x => JsonSerializer.Deserialize<GradingTask>(x))
                    .Where(x => x != null)
                    .GroupBy(x => new { x!.UserId, x.ExamId })
                    .Select(g => g.Last()) // Keep only the latest task for each user-exam pair
                    .ToList();

                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var examRepo = scope.ServiceProvider.GetRequiredService<Zlearn.V2.Application.Exams.IExamRepo>();
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<ExamHub>>();

                foreach (var task in tasks)
                {
                    try
                    {
                        var participant = await dbContext.Set<ExamParticipant>()
                            .Include(ep => ep.Exam)
                            .FirstOrDefaultAsync(ep => ep.UserId == task!.UserId && ep.ExamId == task.ExamId);

                        if (participant == null || participant.Status == ParticipantStatus.Completed || participant.Status == ParticipantStatus.NotAllowed)
                        {
                            continue;
                        }

                        // Load grading keys (correct keys) from cache or DB
                        var gradingKeys = await examRepo.GetExamGradingKeysAsync(task!.ExamId);
                        if (gradingKeys.Count == 0)
                        {
                            _logger.LogWarning("V2: No grading keys found for exam {examId}.", task.ExamId);
                            continue;
                        }

                        // Load full merged answers from Redis cache or database fallback
                        var tempAnswersKey = $"{task.ExamId}:{task.UserId}";
                        var answersJson = await _redisService.Get(RedisKeys.EXAM_TEMP_ANSWERS, tempAnswersKey);
                        List<SubmitAnswerDto>? submittedAnswers = null;
                        if (!string.IsNullOrEmpty(answersJson))
                        {
                            submittedAnswers = JsonSerializer.Deserialize<List<SubmitAnswerDto>>(answersJson);
                        }
                        else
                        {
                            var dbAnswersJson = await examRepo.GetSelectedAnswersAsync(task.ExamId, task.UserId);
                            if (!string.IsNullOrEmpty(dbAnswersJson))
                            {
                                submittedAnswers = JsonSerializer.Deserialize<List<SubmitAnswerDto>>(dbAnswersJson);
                            }
                        }

                        if (submittedAnswers == null)
                        {
                            continue;
                        }

                        int correctCount = 0;
                        foreach (var answer in submittedAnswers)
                        {
                            if (!gradingKeys.TryGetValue(answer.QuestionId, out var corrects)) continue;
                            var submitted = answer.SubmitKeys ?? new List<int>();
                            var correctKeys = corrects ?? new List<int>();
                            if (submitted.Count == correctKeys.Count && !submitted.Except(correctKeys).Any())
                            {
                                correctCount++;
                            }
                        }

                        var score = Math.Round((double)correctCount / gradingKeys.Count * 10, 2);

                        // Call domain V2 method to submit result
                        participant.Submit(
                            submittedAnswers.Count,
                            correctCount,
                            score,
                            StringHelper.ObjectToJsonString(submittedAnswers),
                            DateTimeOffset.UtcNow,
                            ParticipantStatus.Completed
                        );

                        dbContext.Set<ExamParticipant>().Update(participant);
                        await dbContext.SaveChangesAsync();

                        // Notify the teacher via SignalR progress update
                        await hubContext.Clients.Group(ExamHub.GetExamOwnerGroupName(task.ExamId))
                            .SendAsync("ParticipantSelectedAnswer", task.UserId, submittedAnswers.Count);

                        _logger.LogInformation("V2: Successfully background graded exam {examId} for user {userId}. Score: {score}", task.ExamId, task.UserId, score);

                        // Trigger exam completion check & 5-minute result caching if all completed
                        await examRepo.FinalizeExamIfAllCompletedAsync(task.ExamId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "V2: Error processing grading task for user {userId} in exam {examId}.", task!.UserId, task.ExamId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "V2: Error in exam grading background service execution loop.");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("V2 Exam grading background service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            await base.StopAsync(cancellationToken);
        }
    }
}
