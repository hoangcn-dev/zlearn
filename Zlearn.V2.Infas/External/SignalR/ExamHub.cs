using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Common.Utils;
using Zlearn.V2.Application.Exams;
using Zlearn.V2.Application.Exams.DTOs;
using Zlearn.V2.Domain.ExamContext.Exams;
using Zlearn.V2.Domain.ExamContext.Participants;

namespace Zlearn.V2.Infas.External.SignalR
{
    public class ExamHub : Hub
    {
        private readonly IExamRepo _examRepo;
        private readonly IRedisService _redisService;
        private readonly IExamTrackingService _examTrackingService;

        public const string HUB_URL = "/hubs/exam";

        public ExamHub(
            IRedisService redisService, 
            IExamRepo examRepo, 
            IExamTrackingService examTrackingService)
        {
            _redisService = redisService;
            _examRepo = examRepo;
            _examTrackingService = examTrackingService;
        }

        public async Task AnswerSelected(int qCount)
        {
            var userId = Context.User!.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
            {
                var token = Context.GetHttpContext()?.Request.Cookies["exam_session_token"];
                if (!string.IsNullOrEmpty(token))
                {
                    var session = await _redisService.GetObject<ExamSessionDto>(RedisKeys.EXAM_SESSION, token);
                    if (session is not null)
                    {
                        userId = session.u;
                    }
                }
            }

            if (userId is not null)
            {
                var examId = await _redisService.Get(RedisKeys.EXAM_PARTICIPANT_MAP, userId);
                if (examId is not null)
                {
                    await Clients.Group(GetExamOwnerGroupName(examId)).SendAsync("ParticipantSelectedAnswer", userId, qCount);
                }
            }
        }

        public async Task SyncAnswers(SubmitExamDto data, int seq)
        {
            var userId = Context.User!.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
            {
                var token = Context.GetHttpContext()?.Request.Cookies["exam_session_token"];
                if (!string.IsNullOrEmpty(token))
                {
                    var session = await _redisService.GetObject<ExamSessionDto>(RedisKeys.EXAM_SESSION, token);
                    if (session is not null)
                    {
                        userId = session.u;
                    }
                }
            }

            if (userId is null || string.IsNullOrEmpty(data.ExamId))
            {
                return;
            }

            var tempAnswersKey = $"{data.ExamId}:{userId}";

            var existingJson = await _redisService.Get(RedisKeys.EXAM_TEMP_ANSWERS, tempAnswersKey);
            List<SubmitAnswerDto> mergedAnswers;
            if (!string.IsNullOrEmpty(existingJson))
            {
                mergedAnswers = JsonSerializer.Deserialize<List<SubmitAnswerDto>>(existingJson) ?? new();
            }
            else
            {
                var dbAnswersJson = await _examRepo.GetSelectedAnswersAsync(data.ExamId, userId);
                if (!string.IsNullOrEmpty(dbAnswersJson))
                {
                    mergedAnswers = JsonSerializer.Deserialize<List<SubmitAnswerDto>>(dbAnswersJson) ?? new();
                }
                else
                {
                    mergedAnswers = new();
                }
            }

            foreach (var deltaAns in data.Answers)
            {
                var existingAns = mergedAnswers.FirstOrDefault(a => a.QuestionId == deltaAns.QuestionId);
                if (existingAns != null)
                {
                    existingAns.SubmitKeys = deltaAns.SubmitKeys;
                }
                else
                {
                    mergedAnswers.Add(deltaAns);
                }
            }

            var answersJson = JsonSerializer.Serialize(mergedAnswers);
            await _redisService.Set(RedisKeys.EXAM_TEMP_ANSWERS, tempAnswersKey, answersJson, TimeSpan.FromHours(12));

            var taskPayload = new GradingTask
            {
                UserId = userId,
                ExamId = data.ExamId,
                Seq = seq
            };
            var taskJson = JsonSerializer.Serialize(taskPayload);
            await _redisService.ListPush(RedisKeys.EXAM_GRADING_QUEUE, "global", taskJson);

            await Clients.Caller.SendAsync("SyncAck", seq);
        }

        public async Task UpdateProgress(string examId)
        {
            var userId = Context.User!.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is not null && await _examRepo.IsExamCreator(examId, userId))
            {
                await Clients.Group(GetExamParticipantGroupName(examId)).SendAsync("RequireUpdateProgress");
            }
        }

        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var token = httpContext?.Request.Cookies["exam_session_token"];

            if (!string.IsNullOrEmpty(token))
            {
                var session = await _redisService.GetObject<ExamSessionDto>(RedisKeys.EXAM_SESSION, token);
                if (session is not null)
                {
                    var userId = session.u;
                    var examId = session.e;

                    if (await _examRepo.AnyAsync(e => e.Id == examId && e.Status == ExamStatus.Ended))
                    {
                        Context.Abort();
                        return;
                    }

                    await Groups.AddToGroupAsync(Context.ConnectionId, GetExamParticipantGroupName(examId));
                    await _redisService.Set(RedisKeys.EXAM_PARTICIPANT_MAP, userId, examId, TimeSpan.FromHours(12));
                    await _redisService.Delete(RedisKeys.EXAM_SESSION_DISCONNECT, userId);

                    await Clients.Caller.SendAsync("Connected", "Connected!");
                    await base.OnConnectedAsync();
                    return;
                }
            }

            if (Context.User is not null && Context.User.Identity!.IsAuthenticated)
            {
                var examId = httpContext?.Request.Query["examId"].ToString();
                var userId = Context.User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(examId) && userId is not null && await _examRepo.IsExamCreator(examId, userId))
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, GetExamOwnerGroupName(examId));
                    await Clients.Caller.SendAsync("Connected", "Connected!");
                    await base.OnConnectedAsync();
                    return;
                }
            }

            Context.Abort();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null)
            {
                var token = Context.GetHttpContext()?.Request.Cookies["exam_session_token"];
                if (!string.IsNullOrEmpty(token))
                {
                    var session = await _redisService.GetObject<ExamSessionDto>(RedisKeys.EXAM_SESSION, token);
                    if (session is not null)
                    {
                        userId = session.u;
                    }
                }
            }

            if (userId is not null)
            {
                var examId = await _redisService.Get(RedisKeys.EXAM_PARTICIPANT_MAP, userId);
                if (examId is not null)
                {
                    await _redisService.Delete(RedisKeys.EXAM_PARTICIPANT_MAP, userId);

                    var disconnectTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
                    await _redisService.Set(RedisKeys.EXAM_SESSION_DISCONNECT, userId, $"{examId}:{disconnectTime}", TimeSpan.FromSeconds(30));

                    _ = Task.Run(async () =>
                    {
                        await Task.Delay(15000);
                        var disconnectVal = await _redisService.Get(RedisKeys.EXAM_SESSION_DISCONNECT, userId);
                        if (disconnectVal is not null)
                        {
                            var parts = disconnectVal.Split(':');
                            if (parts.Length > 0)
                            {
                                var targetExamId = parts[0];
                                var status = await _examRepo.SetParticipantStatus(userId, targetExamId, ParticipantStatus.ConnectionLost);
                                await _examTrackingService.UpdateParticipantStatus(targetExamId, userId, status);
                            }
                            await _redisService.Delete(RedisKeys.EXAM_SESSION_DISCONNECT, userId);
                        }
                    });
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public static string GetExamOwnerGroupName(string examId) => $"{examId}_owner";
        public static string GetExamParticipantGroupName(string examId) => $"{examId}_participant";
    }
}
