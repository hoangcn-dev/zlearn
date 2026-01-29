using System.Text.Json;
using ZLearn.Application.Common.Queries;
using ZLearn.Application.Common.Services;
using ZLearn.Application.Quizzes.DTOs;
namespace ZLearn.Application.Quizzes.Queries.GetAutoGenerateQuestionData
{
    public class GetAutoGenerateQuestionDataQueryHandler : BaseQueryHandler, IRequestHandler<GetAutoGenerateQuestionDataQuery, List<CreateQuestionDto>>
    {
        private readonly IAIService _aIService;

        public GetAutoGenerateQuestionDataQueryHandler(
            IMapper mapper,
            IMediator mediator,
            IAIService aIService) : base(mapper, mediator)
        {
            _aIService = aIService;
        }

        public async Task<List<CreateQuestionDto>> Handle(GetAutoGenerateQuestionDataQuery request, CancellationToken cancellationToken)

        {
            var rolePrompt = "Bạn là trợ lý tạo câu hỏi trắc nghiệm, tôi sẽ gửi cho bạn context/ yêu cầu dạng chữ, số câu hỏi mong muốn được tạo. Nhiệm vụ của bạn sau khi tạo là trả về dữ liệu dạng array json (Output must be valid JSON only, no markdown, no explanations.), mỗi element trong json có cấu trúc tương tự như sau: {\"order\": 1, \"stringContent\": \"Nội dung câu hỏi 1\", \"correctKey\": 1, \"answers\": [{\"key\": 1, \"stringContent\": \"Nội dung câu trả lời cho câu 1\"}, {\"key\": 2, \"stringContent\": \"Nội dung câu trả lời cho câu 1\"}, {\"key\": 3, \"stringContent\": \"Nội dung câu trả lời cho câu 1\"}, {\"key\": 4, \"stringContent\": \"Nội dung câu trả lời cho câu 1\"}]}";
            var requirement = request.Data.PromptContext;
            var stringData = await _aIService.PromptAndAsk(rolePrompt, requirement);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var questions = JsonSerializer.Deserialize<List<CreateQuestionDto>>(stringData, options)
                ?? throw new BadRequestException("Tạo thất bại, vui lòng thử lại hoặc đổi yêu cầu prompt. Hãy nêu yêu cầu rõ ràng nhất có thể");
            return questions;
        }
    }
}
