using System.Security.Claims;
using ZLearn.API.Exceptions;
using ZLearn.Application.Common.Utils;
using ZLearn.Application.Exams;
using ZLearn.Application.Exams.DTOs;
using ZLearn.Application.Files;
using ZLearn.Application.Quizzes.DTOs;
using ZLearn.Domain.Entities;
using ZLearn.Domain.Enums;

namespace ZLearn.Infras.Data.Repositories
{
    public class ExamRepo : BaseRepo<Exam>, IExamRepo
    {
        public ExamRepo(AppDbContext context) : base(context)
        {
        }

        public async Task<ParticipantWaitingInfoDto> AddParticipant(ClaimsPrincipal user, JoinExamRequestDto data)
        {
            var query = _context.Set<ExamParticipant>();

            var exam = await _context.Set<Exam>()
                .AsNoTracking()
                .Where(e => e.Id == data.ExamId)
                .Select(e => new { 
                    e.Status, 
                    e.JoinPass,
                    e.StartTime,
                    Participants = e.Participants.Select(p => new { p.Id , p.ParticipantCode }), 
                    e.RequireJoinWithCode,
                    e.RequireJoinWithName,
                    e.MaxParticipants })
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(Exam));

            if (exam.Status == ExamStatus.Ended)
                throw new BadRequestException("Bài kiểm tra đã kết thúc.");

            if (exam.Participants.Count() == exam.MaxParticipants)
                throw new BadRequestException("Bài kiểm tra đã đủ số lượng người tham gia.");

            if (exam.RequireJoinWithCode && string.IsNullOrEmpty(data.ParticipantCode))
                throw new BadRequestException("Vui lòng nhập mã tham gia.");

            if (exam.RequireJoinWithName && string.IsNullOrEmpty(data.ParticipantName))
                throw new BadRequestException("Vui lòng nhập tên tham gia.");

            if (exam.Participants.Any(p => p.Id == user.FindFirstValue(ClaimTypes.NameIdentifier)))
                throw new BadRequestException("Bạn đã tham gia bài kiểm tra, vui lòng tải lại trang để cập nhật trạng thái");

            if (!string.IsNullOrEmpty(exam.JoinPass) && exam.JoinPass != data.Password)
                throw new BadRequestException("Mật khẩu tham gia không đúng, vui lòng thử lại");

            if (exam.RequireJoinWithCode && exam.Participants.Any(p => p.ParticipantCode == data.ParticipantCode))
                throw new BadRequestException("Mã tham gia đã được sử dụng, vui lòng thử lại");

            var participant = new ExamParticipant
            {
                Id = IdGenerator.Generate("EPA"),
                UserId = user.FindFirstValue(ClaimTypes.NameIdentifier),
                ExamId = data.ExamId,
                ParticipantCode = data.ParticipantCode,
                ParticipantName = data.ParticipantName ?? $"{user.FindFirstValue("LastName")} {user.FindFirstValue("FirstName")}",
                Status = ParticipantStatus.WaitingForExamStart,
                IsBanned = false,
            };
            _context.Set<ExamParticipant>().Add(participant);
            await _context.SaveChangesAsync();

            return new ParticipantWaitingInfoDto
            {
                ParticipantCode = participant.ParticipantCode,
                ParticipantName = participant.ParticipantName,
                WaitTimeInSeconds = (long)(exam.StartTime - DateTimeOffset.UtcNow).TotalSeconds,
            };
        }

        public async Task<ExamContentDto?> GetExamContentAsync(string alias, string userId)
        {
            // Check and set participant status 
            var ep = await _context.Set<ExamParticipant>().FirstOrDefaultAsync(ep => ep.UserId == userId && ep.Exam.Alias == alias)
                ?? throw new ForbiddenException();
            if (ep.Status == ParticipantStatus.WaitingForExamStart)
            {
                ep.Status = ParticipantStatus.InProgress;
                ep.FirstCheckIn = DateTimeOffset.UtcNow;
            }
            else if (ep.Status == ParticipantStatus.Completed)
            {
                return null;
            }

            // Get exam data
            var exam = await _context.Set<Exam>().AsNoTracking()
                .Where(e => e.Alias == alias)
                .Include(e => e.Quiz)
                    .ThenInclude(q => q.Questions)
                        .ThenInclude(q => q.Answers)
                .FirstOrDefaultAsync();
            if (exam is null) return null;
            var data = new ExamContentDto
            {
                Id = exam.Id,
                ExamName = exam.Name,
                Alias = exam.Alias,
                StartTime = exam.StartTime,
                EndTime = exam.EndTime,
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
                    }).ToList()
                }).ToList()
            };

            for (int i = 0; i < data.Questions.Count; i++)
            {
                var question = exam.Quiz.Questions[i];
                foreach (var url in question.MediaFileUrls.Split(","))
                {
                    if (string.IsNullOrEmpty(url)) continue;
                    var mediaType = FileHelper.GetMediaType(url);
                    if (mediaType == MediaType.Image)
                    {
                        data.Questions[i].ImageUrls.Add(url);
                    }
                    else if (mediaType == MediaType.Audio)
                    {
                        data.Questions[i].AudioUrls.Add(url);
                    }
                    else if (mediaType == MediaType.Video)
                    {
                        data.Questions[i].VideoUrls.Add(url);
                    }
                }

                for (int j = 0; j < data.Questions[i].Answers.Count; j++)
                {
                    data.Questions[i].Answers[j].Label = StringHelper.IndexToChar(j);
                }
            }

            data.Questions.Sort((a, b) => a.Order.CompareTo(b.Order));

            await _context.SaveChangesAsync();
            return data;
        }

        public async Task<ParticipantWaitingInfoDto?> GetParticipantStatusAsync(string userId, string alias)
        {
            var res = await _context.Set<ExamParticipant>()
                .AsNoTracking()
                .Where(ep => ep.UserId == userId && ep.Exam.Alias == alias)
                .Select(ep => new 
                {
                    ep.Status,
                    ep.ParticipantCode,
                    ep.ParticipantName,
                    ep.Exam.StartTime
                })
                .FirstOrDefaultAsync();
            if (res == null) return null;
            var status = new ParticipantWaitingInfoDto
            {
                Status = res.Status,
                ParticipantCode = res.ParticipantCode,
                ParticipantName = res.ParticipantName!,
                WaitTimeInSeconds = (long)(res.StartTime - DateTimeOffset.UtcNow).TotalSeconds
            };
            return status;
        }

        public async Task<ParticipantResultDto?> GetResult(string participantId, string alias)
        {
            var participant = await _context.Set<ExamParticipant>()
                .AsNoTracking()
                .Where(ep => ep.UserId == participantId && ep.Exam.Alias == alias)
                .Select(ep => new
                {
                    ep.FirstCheckIn,
                    ep.LastCheckOut,
                    ep.Correct,
                    ep.Score,
                    ep.ExamId,
                    ep.Completed,
                    QuizId = ep.Exam.QuizId
                })
                .FirstOrDefaultAsync() ?? throw new NotFoundException(nameof(ExamParticipant));

            var rank = await _context.Set<ExamParticipant>()
                .AsNoTracking()
                .Where(ep => ep.ExamId == participant.ExamId && ep.Score > participant.Score)
                .CountAsync() + 1;

            var participantsCount = await _context.Set<ExamParticipant>()
                .AsNoTracking()
                .CountAsync(ep => ep.ExamId == participant.ExamId);

            var questionsCount = await _context.Set<Question>()
                .AsNoTracking()
                .CountAsync(q => q.QuizId == participant.QuizId);

            return new ParticipantResultDto
            {
                FirstCheckIn = participant.FirstCheckIn!.Value,
                LastCheckOut = participant.LastCheckOut!.Value,
                CorrectCount = participant.Correct,
                CompletedCount = participant.Completed,
                Score = participant.Score,
                ParticipantsCount = participantsCount,
                QuestionsCount = questionsCount,
                Rank = rank
            };
        }

        public async Task SaveResult(string participantId, SubmitExamDto data)
        {
            var participant = await _context.Set<ExamParticipant>()
                .Where(ep => ep.UserId == participantId && ep.ExamId == data.ExamId)
                .FirstOrDefaultAsync()
                ?? throw new NotFoundException(nameof(ExamParticipant));
            var questions = await _context.Set<Exam>()
                .Where(e => e.Id == data.ExamId)
                .SelectMany(e => e.Quiz.Questions)
                .Select(q => new { q.Id, q.CorrectKey })
                .ToDictionaryAsync(keySelector: q => q.Id);
            if (questions.Count == 0) 
                throw new InternalErrorException("Question data is empty.");

            participant.LastCheckOut = DateTimeOffset.UtcNow;
            int correctCount = 0;
            foreach (var answer in data.Answers)
            {
                if (!questions.ContainsKey(answer.QuestionId)) continue;
                var question = questions[answer.QuestionId];
                if (question.CorrectKey == answer.SubmitKey)
                {
                    correctCount++;
                }
            }
            participant.Score = Math.Round((double)correctCount / questions.Count * 10, 2);
            participant.Correct = correctCount;
            participant.Completed = data.Answers.Count;
            participant.Status = ParticipantStatus.Completed;
            _context.Set<ExamParticipant>().Update(participant);
            await _context.SaveChangesAsync();
        }
    }
}
