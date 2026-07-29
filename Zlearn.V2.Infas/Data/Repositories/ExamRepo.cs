using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Zlearn.V2.Application.Common.Exceptions;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Application.Common.Interfaces;
using Zlearn.V2.Application.Common.Utils;
using Zlearn.V2.Application.Exams.DTOs;
using Zlearn.V2.Application.Quizzes.DTOs;
using Zlearn.V2.Application.Exams;
using Zlearn.V2.Domain.CatalogContext.Questions;
using Zlearn.V2.Domain.ExamContext.Exams;
using Zlearn.V2.Domain.ExamContext.Participants;
using Zlearn.V2.Domain.FileContext.MediaFiles;
using ParticipantStatus = Zlearn.V2.Domain.ExamContext.Participants.ParticipantStatus;
using ExamStatus = Zlearn.V2.Domain.ExamContext.Exams.ExamStatus;
using MediaType = Zlearn.V2.Domain.FileContext.MediaFiles.MediaType;
using Zlearn.V2.Domain.Common;

namespace Zlearn.V2.Infas.Data.Repositories
{
    public class ExamRepo : WriteRepo<Exam>, IExamRepo
    {
        private readonly ISchedulerService _schedulerService;
        private readonly IRedisService _redisService;
        private readonly IMemoryCache _memoryCache;

        public ExamRepo(
            AppDbContext context,
            ISchedulerService schedulerService,
            IRedisService redisService,
            IMemoryCache memoryCache) : base(context)
        {
            _schedulerService = schedulerService;
            _redisService = redisService;
            _memoryCache = memoryCache;
        }

        public async Task<Exam?> Get(System.Linq.Expressions.Expression<System.Func<Exam, bool>> filter)
        {
            return await _context.Set<Exam>().FirstOrDefaultAsync(filter);
        }

        public async Task<TProjection?> Get<TProjection>(
            System.Linq.Expressions.Expression<System.Func<Exam, bool>> filter,
            System.Linq.Expressions.Expression<System.Func<Exam, TProjection>> projector)
        {
            return await _context.Set<Exam>()
                .Where(filter)
                .Select(projector)
                .FirstOrDefaultAsync();
        }

        public async Task<List<TProjection>> GetAll<TProjection>(
            System.Linq.Expressions.Expression<System.Func<Exam, bool>> filter,
            System.Linq.Expressions.Expression<System.Func<Exam, TProjection>> projector,
            System.Linq.Expressions.Expression<System.Func<Exam, object>>? orderBy = null,
            bool isAsc = true)
        {
            var query = _context.Set<Exam>().Where(filter);
            if (orderBy != null)
            {
                query = isAsc ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
            }
            return await query.Select(projector).ToListAsync();
        }

        public async Task<ParticipantWaitingInfoDto> AddParticipant(ClaimsPrincipal user, JoinExamRequestDto data)
        {
            var exam = await _context.Set<Exam>()
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Id == data.ExamId)
                ?? throw new NotFoundException("Bài kiểm tra không tồn tại hoặc đã hết thời gian cho phép tham gia.");

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var participantName = data.ParticipantName ?? $"{user.FindFirstValue("LastName")} {user.FindFirstValue("FirstName")}";

            // Call domain method on Exam Aggregate Root
            ExamParticipant participant;
            try
            {
                participant = exam.AddParticipant(userId, participantName, data.ParticipantCode, data.Password);
            }
            catch (InvalidOperationException ex)
            {
                throw new BadRequestException(ex.Message);
            }

            await _context.SaveChangesAsync();

            return new ParticipantWaitingInfoDto
            {
                ParticipantId = participant.Id,
                Status = participant.Status,
                ParticipantCode = participant.ParticipantCode,
                ParticipantName = participant.ParticipantName,
                StartTime = exam.StartTime,
                EndTime = exam.EndTime,
                ExamStatus = exam.Status,
            };
        }

        public async Task<(ExamContentDto, ExamParticipant)?> GetExamContentAsync(string alias, string userId)
        {
            var ep = await _context.Set<ExamParticipant>()
                .Include(x => x.Exam)
                .FirstOrDefaultAsync(ep => ep.UserId == userId && ep.Exam.Alias == alias)
                ?? throw new ForbiddenException();

            if (ep.Status == ParticipantStatus.Completed)
            {
                throw new RedirectException($"/bai-kiem-tra/result?alias={alias}");
            }
            if (ep.Status == ParticipantStatus.NotAllowed)
            {
                throw new ForbiddenException();
            }

            // Call domain method to check in
            ep.CheckIn(DateTimeOffset.UtcNow);

            string? examJson = null;
            string cacheKey = $"EXAM_RAW_CONTENT_{alias}";

            if (_memoryCache.TryGetValue(cacheKey, out string? cachedJson))
            {
                examJson = cachedJson;
            }
            else
            {
                examJson = await _redisService.Get(RedisKeys.EXAM_RAW_CONTENT, alias);
                if (!string.IsNullOrEmpty(examJson))
                {
                    _memoryCache.Set(cacheKey, examJson, TimeSpan.FromMinutes(2));
                }
            }

            if (string.IsNullOrEmpty(examJson))
            {
                var exam = await _context.Set<Exam>().AsNoTracking()
                    .Where(e => e.Alias == alias && e.Status == ExamStatus.InProgress)
                    .Include(e => e.Quiz)
                        .ThenInclude(q => q.Questions)
                            .ThenInclude(q => q.Answers)
                    .FirstOrDefaultAsync();
                if (exam is null) return null;

                var rawData = new ExamContentDto
                {
                    Id = exam.Id,
                    ExamName = exam.Name,
                    Alias = exam.Alias,
                    StartTime = exam.StartTime,
                    EndTime = exam.EndTime,
                    MixQuestions = exam.MixQuestions,
                    MixAnswers = exam.MixAnswers,
                    Questions = exam.Quiz.Questions.Select(question => new QuestionContentDto
                    {
                        Id = question.Id,
                        StringContent = question.StringContent,
                        QuizId = question.QuizId,
                        Slug = question.Slug,
                        QuizName = question.Quiz.Name,
                        AttemptCount = question.AttemptCount,
                        Order = question.Order,
                        Answers = question.Answers.Select(a => new AnswerContentDto
                        {
                            Key = a.Key,
                            StringContent = a.StringContent,
                            ImageUrls = a.MediaFileUrls.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList()
                        }).ToList(),
                        IsMultipleChoice = question.Answers.Count(a => a.IsCorrect) > 1
                    }).ToList()
                };

                for (int i = 0; i < rawData.Questions.Count; i++)
                {
                    var question = exam.Quiz.Questions[i];
                    foreach (var url in question.MediaFileUrls.Split(','))
                    {
                        if (string.IsNullOrEmpty(url)) continue;
                        var mediaType = Zlearn.V2.Application.Files.FileHelper.GetMediaType(url);
                        if (mediaType == MediaType.Image)
                        {
                            rawData.Questions[i].ImageUrls.Add(url);
                        }
                        else if (mediaType == MediaType.Audio)
                        {
                            rawData.Questions[i].AudioUrls.Add(url);
                        }
                        else if (mediaType == MediaType.Video)
                        {
                            rawData.Questions[i].VideoUrls.Add(url);
                        }
                    }
                }

                examJson = JsonSerializer.Serialize(rawData);
                var redisTtl = TimeSpan.FromHours(2);
                if (exam.EndTime.HasValue)
                {
                    var diff = exam.EndTime.Value - DateTimeOffset.UtcNow;
                    redisTtl = diff.TotalSeconds > 0 ? diff : TimeSpan.FromSeconds(1);
                }
                await _redisService.Set(RedisKeys.EXAM_RAW_CONTENT, alias, examJson, redisTtl);
                _memoryCache.Set(cacheKey, examJson, TimeSpan.FromMinutes(2));
            }

            var data = JsonSerializer.Deserialize<ExamContentDto>(examJson)!;
            data.ParticipantCode = ep.ParticipantCode;
            data.ParticipantName = ep.ParticipantName;

            for (int i = 0; i < data.Questions.Count; i++)
            {
                if (data.MixAnswers)
                {
                    var rnd = new Random();
                    data.Questions[i].Answers = data.Questions[i].Answers.OrderBy(x => rnd.Next()).ToList();
                }
                for (int j = 0; j < data.Questions[i].Answers.Count; j++)
                {
                    data.Questions[i].Answers[j].Label = StringHelper.IndexToChar(j);
                }
            }

            if (data.MixQuestions)
            {
                var rnd = new Random();
                data.Questions = data.Questions.OrderBy(x => rnd.Next()).ToList();
                for (int i = 0; i < data.Questions.Count; i++)
                {
                    data.Questions[i].Order = i + 1;
                }
            }
            else
            {
                data.Questions.Sort((a, b) => a.Order.CompareTo(b.Order));
            }

            _context.Set<ExamParticipant>().Update(ep);
            await _context.SaveChangesAsync();
            return (data, ep);
        }

        public async Task<ParticipantWaitingInfoDto?> GetParticipantStatusAsync(string userId, string alias)
        {
            var res = await _context.Set<ExamParticipant>()
                .AsNoTracking()
                .Where(ep => ep.UserId == userId && ep.Exam.Alias == alias)
                .Select(ep => new 
                {
                    ep.Id,
                    ep.Status,
                    ep.ParticipantCode,
                    ep.ParticipantName,
                    ep.Exam.StartTime,
                    ep.Exam.EndTime,
                    ExamStatus = ep.Exam.Status,
                })
                .FirstOrDefaultAsync();

            if (res == null) return null;

            return new ParticipantWaitingInfoDto
            {
                ParticipantId = res.Id,
                Status = res.Status,
                ParticipantCode = res.ParticipantCode,
                ParticipantName = res.ParticipantName,
                StartTime = res.StartTime,
                EndTime = res.EndTime,
                ExamStatus = res.ExamStatus,
            };
        }

        public async Task<ParticipantResultDto?> GetResult(string participantId, string alias)
        {
            var examInfo = await _context.Set<Exam>()
                .AsNoTracking()
                .Where(e => e.Alias == alias)
                .Select(e => new { e.Id, e.QuizId })
                .FirstOrDefaultAsync() ?? throw new NotFoundException(nameof(Exam));

            var resultCacheKey = $"{examInfo.Id}:{participantId}";
            var cachedJson = await _redisService.Get(RedisKeys.EXAM_RESULT, resultCacheKey);
            if (!string.IsNullOrEmpty(cachedJson))
            {
                var cachedResult = JsonSerializer.Deserialize<GradedResultCacheDto>(cachedJson);
                if (cachedResult != null)
                {
                    var rank = await _context.Set<ExamParticipant>()
                        .AsNoTracking()
                        .Where(ep => ep.ExamId == examInfo.Id && ep.Status == ParticipantStatus.Completed && ep.Score > cachedResult.Score)
                        .CountAsync() + 1;

                    var participantsCount = await _context.Set<ExamParticipant>()
                        .AsNoTracking()
                        .CountAsync(ep => ep.ExamId == examInfo.Id);

                    var questionsCount = await _context.Set<Question>()
                        .AsNoTracking()
                        .CountAsync(q => q.QuizId == examInfo.QuizId);

                    return new ParticipantResultDto
                    {
                        FirstCheckIn = cachedResult.FirstCheckIn,
                        LastCheckOut = cachedResult.LastCheckOut,
                        CorrectCount = cachedResult.Correct,
                        CompletedCount = cachedResult.Completed,
                        Score = cachedResult.Score,
                        ParticipantsCount = participantsCount,
                        QuestionsCount = questionsCount,
                        Rank = rank
                    };
                }
            }

            var participant = await _context.Set<ExamParticipant>()
                .AsNoTracking()
                .Where(ep => ep.UserId == participantId && ep.Exam.Alias == alias && ep.Status == ParticipantStatus.Completed)
                .Select(ep => new
                {
                    ep.FirstCheckIn,
                    ep.LastCheckOut,
                    ep.Correct,
                    ep.Score,
                    ep.ExamId,
                    ep.Completed,
                    ep.Exam.QuizId
                })
                .FirstOrDefaultAsync() ?? throw new NotFoundException(nameof(ExamParticipant));

            var dbRank = await _context.Set<ExamParticipant>()
                .AsNoTracking()
                .Where(ep => ep.ExamId == participant.ExamId && ep.Status == ParticipantStatus.Completed && ep.Score > participant.Score)
                .CountAsync() + 1;

            var dbParticipantsCount = await _context.Set<ExamParticipant>()
                .AsNoTracking()
                .CountAsync(ep => ep.ExamId == participant.ExamId);

            var dbQuestionsCount = await _context.Set<Question>()
                .AsNoTracking()
                .CountAsync(q => q.QuizId == participant.QuizId);

            return new ParticipantResultDto
            {
                FirstCheckIn = participant.FirstCheckIn!.Value,
                LastCheckOut = participant.LastCheckOut!.Value,
                CorrectCount = participant.Correct,
                CompletedCount = participant.Completed,
                Score = participant.Score,
                ParticipantsCount = dbParticipantsCount,
                QuestionsCount = dbQuestionsCount,
                Rank = dbRank
            };
        }

        public Task<bool> IsExamCreator(string examId, string userId)
        {
            return _context.Set<Exam>()
                .AsNoTracking()
                .AnyAsync(e => e.Id == examId && e.CreatedBy == userId);
        }

        public async Task<ParticipantStatusDto?> GetExamParticipant(string examId, string userId)
        {
            return await _context.Set<ExamParticipant>()
                .AsNoTracking()
                .Where(ep => ep.ExamId == examId && ep.UserId == userId)
                .Select(ep => new ParticipantStatusDto
                {
                    ParticipantId = ep.Id,
                    UserId = ep.UserId,
                    ParticipantCode = ep.ParticipantCode,
                    ParticipantName = ep.ParticipantName,
                    Status = ep.Status,
                })
                .FirstOrDefaultAsync();
        }

        public async Task SaveResult(string participantId, SubmitExamDto data)
        {
            var exam = await _context.Set<Exam>()
                .Include(e => e.Participants)
                .Include(e => e.Quiz)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync(e => e.Id == data.ExamId)
                ?? throw new BadRequestException("Bài kiểm tra không tồn tại");

            if (exam.Status == ExamStatus.Ended && !exam.AllowLateSubmit)
                throw new BadRequestException("Bài kiểm tra đã kết thúc.");

            var participant = exam.Participants.FirstOrDefault(ep => ep.UserId == participantId)
                ?? throw new NotFoundException(nameof(ExamParticipant));

            if (participant.Status == ParticipantStatus.NotAllowed)
            {
                throw new ForbiddenException();
            }

            var questions = exam.Quiz.Questions.ToDictionary(q => q.Id);
            if (questions.Count == 0) throw new InternalErrorException("Question data is empty.");

            int correctCount = 0;
            foreach (var answer in data.Answers)
            {
                if (!questions.ContainsKey(answer.QuestionId)) continue;
                var question = questions[answer.QuestionId];
                var submitted = answer.SubmitKeys ?? new List<int>();
                var corrects = question.Answers.Where(a => a.IsCorrect).Select(a => a.Key).ToList();
                if (submitted.Count == corrects.Count && !submitted.Except(corrects).Any())
                {
                    correctCount++;
                }
            }

            var score = Math.Round((double)correctCount / questions.Count * 10, 2);
            var submitStatus = exam.Status == ExamStatus.Ended ? ParticipantStatus.TimeOut : ParticipantStatus.Completed;

            // Call domain method to submit
            participant.Submit(data.Answers.Count, correctCount, score, StringHelper.ObjectToJsonString(data.Answers), DateTimeOffset.UtcNow, submitStatus);

            await _context.SaveChangesAsync();

            // Check if all participants are finished
            var allFinished = exam.Participants.All(ep => ep.Status == ParticipantStatus.Completed || ep.Status == ParticipantStatus.TimeOut || ep.Status == ParticipantStatus.NotAllowed);
            if (allFinished)
            {
                await PerformFinalExamCleanupAndResultCachingAsync(data.ExamId);
            }
        }

        public Task<bool> IsExamParticipant(string examId, string userId)
        {
            return _context.Set<ExamParticipant>()
                .AsNoTracking()
                .AnyAsync(ep => ep.ExamId == examId && ep.UserId == userId);
        }

        public async Task<Zlearn.V2.Domain.ExamContext.Participants.ParticipantStatus> SetParticipantStatus(string userId, string examId, Zlearn.V2.Domain.ExamContext.Participants.ParticipantStatus status)
        {
            var participant = await _context.Set<ExamParticipant>()
                .FirstOrDefaultAsync(ep => ep.UserId == userId && ep.ExamId == examId)
                ?? throw new NotFoundException(nameof(ExamParticipant));

            if (participant.Status != ParticipantStatus.Completed)
            {
                participant.ChangeStatus(status);
            }

            await _context.SaveChangesAsync();
            return participant.Status;
        }

        public async Task EndExam(string userId, string examId)
        {
            var exam = await _context.Set<Exam>()
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Id == examId)
                ?? throw new NotFoundException(nameof(Exam));

            if (exam.CreatedBy != userId) throw new ForbiddenException();

            // Call domain method to end the exam
            exam.End(DateTimeOffset.UtcNow);

            if (exam.EndJobId is not null && await _schedulerService.CancelExactlyScheduleById(exam.EndJobId, exam.Id))
            {
                exam.UpdateJobIds(exam.StartJobId, null);
            }

            await _context.SaveChangesAsync();

            await PerformFinalExamCleanupAndResultCachingAsync(exam.Id);
        }

        public async Task<string> ManageParticipant(string examId, string participantId, ManageParticipantAction action)
        {
            var participant = await _context.Set<ExamParticipant>()
                .FirstOrDefaultAsync(ep => ep.ExamId == examId && ep.Id == participantId && ep.Status != ParticipantStatus.Completed)
                ?? throw new NotFoundException(nameof(ExamParticipant));

            if (action == ManageParticipantAction.Remove)
            {
                _context.Set<ExamParticipant>().Remove(participant);
            }
            else if (action == ManageParticipantAction.Block)
            {
                participant.Ban();
            }
            else if (action == ManageParticipantAction.Unblock)
            {
                participant.Unban();
            }

            if (action == ManageParticipantAction.Remove || action == ManageParticipantAction.Block)
            {
                var userId = participant.UserId;
                var userSessionKey = $"{userId}:{examId}";
                var token = await _redisService.Get(RedisKeys.EXAM_USER_SESSION, userSessionKey);
                if (!string.IsNullOrEmpty(token))
                {
                    await _redisService.Delete(RedisKeys.EXAM_SESSION, token);
                    await _redisService.Delete(RedisKeys.EXAM_USER_SESSION, userSessionKey);
                }
            }

            await _context.SaveChangesAsync();
            return participantId;
        }

        public async Task<List<OnGoingExamListItemDto>> GetOnGoingExams(string userId)
        {
            return await _context.Set<ExamParticipant>()
                .AsNoTracking()
                .Where(ep => ep.UserId == userId && (ep.Exam.Status == ExamStatus.InProgress || ep.Exam.Status == ExamStatus.WaitStart))
                .Select(ep => new OnGoingExamListItemDto
                {
                    Id = ep.Exam.Id,
                    Name = ep.Exam.Name,
                    Status = ep.Exam.Status,
                    StartTime = ep.Exam.StartTime,
                    EndTime = ep.Exam.EndTime,
                    Alias = ep.Exam.Alias
                })
                .OrderBy(e => e.Name)
                .ToListAsync();
        }

        public async Task<FileDataDto> GetExamScoreAsExcel(string examId, string userId)
        {
            var exam = await _context.Set<Exam>().AsNoTracking()
                .Where(e => e.Id.Equals(examId))
                .Select(e => new
                {
                    e.Name,
                    Participants = e.Participants
                        .Where(p => p.Status == ParticipantStatus.Completed || (e.AllowLateSubmit && p.Status == ParticipantStatus.TimeOut))
                        .Select(p => new
                        {
                            p.Id,
                            p.ParticipantCode,
                            p.ParticipantName,
                            p.Score
                        }).ToList(),
                    e.CreatedBy
                })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Exam), examId);

            if (exam.CreatedBy != userId) throw new ForbiddenException();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(exam.Name);
            ws.Cell(1, 1).Value = "STT";
            ws.Cell(1, 2).Value = "ID";
            ws.Cell(1, 3).Value = "Mã điểm danh";
            ws.Cell(1, 4).Value = "Họ và tên";
            ws.Cell(1, 5).Value = "Điểm";
            for (int i = 0; i < exam.Participants.Count; i++)
            {
                ws.Cell(i + 2, 1).Value = i + 1;
                ws.Cell(i + 2, 2).Value = exam.Participants[i].Id;
                ws.Cell(i + 2, 3).Value = exam.Participants[i].ParticipantCode;
                ws.Cell(i + 2, 4).Value = exam.Participants[i].ParticipantName;
                ws.Cell(i + 2, 5).Value = exam.Participants[i].Score;
            }

            var stream = new MemoryStream();
            wb.SaveAs(stream);
            return new FileDataDto
            {
                FileName = $"Kết quả - {exam.Name}",
                StreamData = stream,
                MIMEType = MIMETypes.XLSX
            };
        }

        public async Task<Dictionary<string, List<int>>> GetExamGradingKeysAsync(string examId)
        {
            string cacheKey = $"EXAM_GRADING_KEYS_{examId}";
            if (_memoryCache.TryGetValue(cacheKey, out Dictionary<string, List<int>>? cachedKeys))
            {
                return cachedKeys!;
            }

            var redisJson = await _redisService.Get(RedisKeys.EXAM_GRADING_KEYS, examId);
            if (!string.IsNullOrEmpty(redisJson))
            {
                var keys = JsonSerializer.Deserialize<Dictionary<string, List<int>>>(redisJson)!;
                _memoryCache.Set(cacheKey, keys, TimeSpan.FromMinutes(5));
                return keys;
            }

            var exam = await _context.Set<Exam>().AsNoTracking()
                .Where(e => e.Id == examId)
                .Select(e => new
                {
                    e.EndTime,
                    Questions = e.Quiz.Questions.Select(q => new { q.Id, CorrectKeys = q.Answers.Where(a => a.IsCorrect).Select(a => a.Key).ToList() })
                })
                .FirstOrDefaultAsync();

            if (exam == null) return new Dictionary<string, List<int>>();

            var dbKeys = exam.Questions.ToDictionary(q => q.Id, q => q.CorrectKeys);
            var json = JsonSerializer.Serialize(dbKeys);
            var redisTtl = TimeSpan.FromHours(2);
            if (exam.EndTime.HasValue)
            {
                var diff = exam.EndTime.Value - DateTimeOffset.UtcNow;
                redisTtl = diff.TotalSeconds > 0 ? diff : TimeSpan.FromSeconds(1);
            }
            await _redisService.Set(RedisKeys.EXAM_GRADING_KEYS, examId, json, redisTtl);
            _memoryCache.Set(cacheKey, dbKeys, TimeSpan.FromMinutes(5));
            return dbKeys;
        }

        public async Task<string?> GetSelectedAnswersAsync(string examId, string userId)
        {
            return await _context.Set<ExamParticipant>()
                .AsNoTracking()
                .Where(ep => ep.ExamId == examId && ep.UserId == userId)
                .Select(ep => ep.SelectedAnswers)
                .FirstOrDefaultAsync();
        }

        private async Task PerformFinalExamCleanupAndResultCachingAsync(string examId)
        {
            var exam = await _context.Set<Exam>()
                .Include(e => e.Participants)
                .FirstOrDefaultAsync(e => e.Id == examId);

            if (exam == null) return;

            var cacheKey = $"EXAM_RAW_CONTENT_{exam.Alias}";
            _memoryCache.Remove(cacheKey);
            await _redisService.Delete(RedisKeys.EXAM_RAW_CONTENT, exam.Alias);

            _memoryCache.Remove($"EXAM_GRADING_KEYS_{exam.Id}");
            await _redisService.Delete(RedisKeys.EXAM_GRADING_KEYS, exam.Id);

            foreach (var p in exam.Participants)
            {
                var userSessionKey = $"{p.UserId}:{exam.Id}";
                var token = await _redisService.Get(RedisKeys.EXAM_USER_SESSION, userSessionKey);
                if (!string.IsNullOrEmpty(token))
                {
                    await _redisService.Delete(RedisKeys.EXAM_SESSION, token);
                    await _redisService.Delete(RedisKeys.EXAM_USER_SESSION, userSessionKey);
                }

                var tempAnswersKey = $"{exam.Id}:{p.UserId}";
                await _redisService.Delete(RedisKeys.EXAM_TEMP_ANSWERS, tempAnswersKey);

                var resultCache = new GradedResultCacheDto
                {
                    Score = p.Score,
                    Correct = p.Correct,
                    Completed = p.Completed,
                    SelectedAnswers = p.SelectedAnswers,
                    FirstCheckIn = p.FirstCheckIn ?? DateTimeOffset.UtcNow,
                    LastCheckOut = p.LastCheckOut ?? DateTimeOffset.UtcNow
                };
                var resultCacheKey = $"{exam.Id}:{p.UserId}";
                await _redisService.Set(RedisKeys.EXAM_RESULT, resultCacheKey, JsonSerializer.Serialize(resultCache), TimeSpan.FromMinutes(5));
            }
        }

        public async Task FinalizeExamIfAllCompletedAsync(string examId)
        {
            var allFinished = await _context.Set<ExamParticipant>()
                .Where(ep => ep.ExamId == examId)
                .AllAsync(ep => ep.Status == ParticipantStatus.Completed || ep.Status == ParticipantStatus.TimeOut || ep.Status == ParticipantStatus.NotAllowed);

            if (allFinished)
            {
                await PerformFinalExamCleanupAndResultCachingAsync(examId);
            }
        }

        public async Task<ExamContentSimDto?> GetExamContentSimulatedAsync(string alias)
        {
            return await _context.Set<Exam>()
                .AsNoTracking()
                .Where(e => e.Alias == alias)
                .Select(e => new ExamContentSimDto
                {
                    ExamId = e.Id,
                    Name = e.Name,
                    Alias = e.Alias,
                    QuizId = e.QuizId,
                    QuizName = e.Quiz != null ? e.Quiz.Name : string.Empty,
                    CategoryId = e.Quiz != null ? e.Quiz.CategoryId : string.Empty,
                    CategoryName = (e.Quiz != null && e.Quiz.Category != null) ? e.Quiz.Category.Name : string.Empty,
                    Note = e.Note ?? string.Empty,
                    Questions = e.Quiz != null ? e.Quiz.Questions.OrderBy(q => q.Order).Select(q => new QuestionSimDto
                    {
                        QuestionId = q.Id,
                        Content = q.StringContent ?? string.Empty,
                        Explanation = q.Explanation ?? string.Empty,
                        Order = q.Order,
                        Answers = q.Answers.Select(a => new AnswerSimDto
                        {
                            AnswerId = a.Id,
                            Key = a.Key,
                            Content = a.StringContent ?? string.Empty,
                            IsCorrect = a.IsCorrect
                        }).ToList()
                    }).ToList() : new List<QuestionSimDto>()
                })
                .FirstOrDefaultAsync();
        }
    }
}
