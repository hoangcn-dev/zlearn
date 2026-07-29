using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using Zlearn.V2.Application.Exams;
using Zlearn.V2.Domain.CatalogContext.Categories;
using Zlearn.V2.Domain.CatalogContext.Quizzes;
using Zlearn.V2.Domain.CatalogContext.Questions;
using Zlearn.V2.Domain.CatalogContext.Answers;
using Zlearn.V2.Domain.ExamContext.Exams;
using Zlearn.V2.Infas.Data;
using Microsoft.EntityFrameworkCore;
using Zlearn.V2.Application.Exams.Commands.ChangeExamStatus;
using Zlearn.V2.Application.Exams.Commands.CreateExam;
using Zlearn.V2.Application.Exams.Commands.JoinExam;
using Zlearn.V2.Application.Exams.Commands.ManageParticipant;
using Zlearn.V2.Application.Exams.Commands.SubmitAnswer;
using Zlearn.V2.Application.Exams.Queries.GetExamDetail;
using Zlearn.V2.Application.Exams.Queries.GetExamScore;
using Zlearn.V2.Application.Exams.Queries.GetExamScoreExcelFileData;
using Zlearn.V2.Application.Exams.Queries.GetOnGoingExam;
using Zlearn.V2.Application.Exams.Queries.GetParticipantStatus;
using Zlearn.V2.Application.Common.DTOs;
using Zlearn.V2.Application.Exams.DTOs;
using Zlearn.V2.Application.Common.Interfaces;
using CreateResponseDto = Zlearn.V2.Application.Common.DTOs.CreateResponseDto;

namespace ZLearn.Web.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExamsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IExamSessionService _examSessionService;

        public ExamsController(IMediator mediator, IExamSessionService examSessionService)
        {
            _mediator = mediator;
            _examSessionService = examSessionService;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateExam([FromBody] CreateExamDto data)
        {
            var res = await _mediator.Send(new CreateExamCommand
            {
                Data = data,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty
            });
            return Ok(Result<CreateResponseDto>.Success("Tạo bài kiểm tra thành công (V2).", res));
        }

        [HttpGet("{id}/export-result")]
        public async Task<IActionResult> ExportResult(string id)
        {
            var res = await _mediator.Send(new GetExamScoreExcelFileDataQuery
            {
                ExamId = id,
                UserClaims = User
            });
            res.StreamData.Position = 0;
            return File(res.StreamData, res.MIMEType, res.FileName);
        }

        [HttpGet("participant-info")]
        public async Task<IActionResult> GetParticipantInfo([FromQuery] string alias)
        {
            var userId = await _examSessionService.GetUserIdAsync(HttpContext);
            var token = await _examSessionService.GetSessionTokenAsync(HttpContext);

            var res = await _mediator.Send(new GetParticipantStatusQuery
            {
                Alias = alias,
                UserId = userId
            });
            if (res == null) return Ok(Result<NoData>.Failure());

            if (!string.IsNullOrEmpty(token))
            {
                res.SessionToken = token;
            }

            return Ok(Result<ParticipantWaitingInfoDto>.Success("", res));
        }

        [HttpGet("on-going")]
        [Authorize]
        public async Task<IActionResult> GetOnGoingExam()
        {
            var res = await _mediator.Send(new GetOnGoingExamQuery
            {
                UserClaims = User
            });
            return Ok(Result<List<OnGoingExamListItemDto>>.Success("", res));
        }

        [HttpPost("submit")]
        public async Task<IActionResult> Submit([FromBody] SubmitExamDto data)
        {
            var userId = await _examSessionService.GetUserIdAsync(HttpContext);

            await _mediator.Send(new SubmitAnswerCommand
            {
                ParticipantId = userId,
                Data = data
            });

            await _examSessionService.HandleSubmitSessionAsync(HttpContext, userId, data.ExamId);

            return Ok(Result<NoData>.Success());
        }

        [HttpPost("join")]
        public async Task<IActionResult> Join([FromBody] JoinExamRequestDto data)
        {
            var result = await _mediator.Send(new JoinExamCommand
            {
                User = User,
                Data = data
            });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is not null && result is not null)
            {
                await _examSessionService.HandleJoinSessionAsync(HttpContext, userId, data.ExamId, result.ParticipantId, result);
            }

            return Ok(Result<ParticipantWaitingInfoDto>.Success("", result));
        }

        [HttpPut("{id}/status")]
        [Authorize]
        public async Task<IActionResult> ChangeStatus(string id, [FromBody] ChangeExamStatusDto data)
        {
            await _mediator.Send(new ChangeExamStatusCommand
            {
                ExamId = id,
                Data = data,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty
            });
            return Ok(Result<NoData>.Success());
        }

        [HttpPost("{id}/manage-participant")]
        [Authorize]
        public async Task<IActionResult> ManageParticipant(string id, [FromBody] ManageParticipantDto data)
        {
            var participantId = await _mediator.Send(new ManageParticipantCommand
            {
                Data = data,
                ExamId = id,
                UserClaims = User
            });
            return Ok(Result<object>.Success("", new { participantId }));
        }

        [HttpPost("sim-seed")]
        public async Task<IActionResult> SeedExamData([FromServices] AppDbContext context)
        {
            var existing = await context.Set<Exam>().FirstOrDefaultAsync(e => e.Alias == "danh-gia-nang-luc-toan-hoc");
            if (existing != null)
            {
                return Ok("Database already seeded.");
            }

            // Create Category
            var category = new Category("Toán Học", "toan-hoc", "Môn Toán Học");
            context.Set<Category>().Add(category);

            // Create Quiz
            var quiz = new Quiz("Đánh giá năng lực toán học", "danh-gia-nang-luc-toan-hoc", category.Id, true);
            context.Set<Quiz>().Add(quiz);

            // Create Questions and Answers
            for (int i = 1; i <= 10; i++)
            {
                var question = new Question
                {
                    QuizId = quiz.Id,
                    Order = i,
                    Slug = $"cau-hoi-{i}",
                    StringContent = $"Câu hỏi {i}: Tính giá trị của biểu thức {i} x 5 + {i}?",
                    Explanation = $"Lời giải chi tiết câu hỏi {i}."
                };
                context.Set<Question>().Add(question);

                for (int j = 1; j <= 4; j++)
                {
                    var answer = new Answer
                    {
                        QuestionId = question.Id,
                        Key = j,
                        StringContent = $"Đáp án {j} của câu hỏi {i}",
                        IsCorrect = (j == 1) // default option 1 is correct
                    };
                    context.Set<Answer>().Add(answer);
                }
            }

            // Create Exam
            var exam = new Exam(
                name: "Thi thử Đánh giá năng lực toán học",
                alias: "danh-gia-nang-luc-toan-hoc",
                quizId: quiz.Id,
                joinPass: null,
                startTime: DateTimeOffset.UtcNow.AddMinutes(-10),
                endTime: DateTimeOffset.UtcNow.AddDays(10),
                status: ExamStatus.InProgress,
                showAnswerAndKey: true,
                maxParticipants: 1000,
                mixAnswers: true,
                mixQuestions: true,
                requireJoinWithCode: false,
                requireJoinWithName: false,
                allowLateSubmit: true,
                note: "Đề thi thử phục vụ kiểm tra hiệu năng",
                id: null
            );
            context.Set<Exam>().Add(exam);

            await context.SaveChangesAsync();
            return Ok("Seeding completed successfully.");
        }

        [HttpGet("sim-list-aliases")]
        public async Task<IActionResult> ListExamAliases([FromServices] IExamRepo examRepo)
        {
            var aliases = await examRepo.GetAll(e => true, e => e.Alias);
            return Ok(aliases);
        }

        [HttpGet("sim-nocache")]
        public async Task<IActionResult> GetExamContentSimNocache([FromQuery] string alias, [FromServices] IExamRepo examRepo)
        {
            if (string.IsNullOrEmpty(alias))
            {
                return BadRequest("Alias is required.");
            }

            var examContent = await examRepo.GetExamContentSimulatedAsync(alias);
            if (examContent == null)
            {
                return NotFound($"Exam with alias '{alias}' not found.");
            }

            return Ok(examContent);
        }

        [HttpGet("sim-cached")]
        public async Task<IActionResult> GetExamContentSimCached(
            [FromQuery] string alias, 
            [FromServices] IExamRepo examRepo,
            [FromServices] Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache,
            [FromServices] IRedisService redisService)
        {
            if (string.IsNullOrEmpty(alias))
            {
                return BadRequest("Alias is required.");
            }

            string cacheKey = $"EXAM_SIM_CONTENT_{alias}";

            if (memoryCache.TryGetValue(cacheKey, out ExamContentSimDto? cachedContent))
            {
                return Ok(cachedContent);
            }

            try
            {
                cachedContent = await redisService.GetObject<ExamContentSimDto>("EXAM_SIM_CONTENT", alias);
                if (cachedContent != null)
                {
                    memoryCache.Set(cacheKey, cachedContent, TimeSpan.FromMinutes(2));
                    return Ok(cachedContent);
                }
            }
            catch
            {
            }

            var examContent = await examRepo.GetExamContentSimulatedAsync(alias);
            if (examContent == null)
            {
                return NotFound($"Exam with alias '{alias}' not found.");
            }

            try
            {
                memoryCache.Set(cacheKey, examContent, TimeSpan.FromMinutes(2));
                await redisService.SetObject("EXAM_SIM_CONTENT", alias, examContent, TimeSpan.FromMinutes(5));
            }
            catch
            {
            }

            return Ok(examContent);
        }
    }
}
