using MediatR;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using ZLearn.Application.Common.DTOs;
using ZLearn.Application.Common.Interfaces;
using ZLearn.Application.Common.Services;

namespace ZLearn.Application.Quizzes.Commands.ScanQuiz
{
    public class ScanQuizCommandHandler : IRequestHandler<ScanQuizCommand, Result<List<ScanQuestionDto>>>
    {
        private readonly IDocumentImportService _importService;
        private readonly IAIService _aiService;

        public ScanQuizCommandHandler(IDocumentImportService importService, IAIService aiService)
        {
            _importService = importService;
            _aiService = aiService;
        }

        public async Task<Result<List<ScanQuestionDto>>> Handle(ScanQuizCommand request, CancellationToken cancellationToken)
        {
            if (request.File == null || request.File.Length == 0)
            {
                return Result<List<ScanQuestionDto>>.Failure("File is empty or invalid.");
            }

            string rawText;
            try
            {
                using var stream = request.File.OpenReadStream();
                rawText = await _importService.ExtractTextFromFileAsync(stream, request.File.ContentType);
            }
            catch (Exception ex)
            {
                return Result<List<ScanQuestionDto>>.Failure($"Failed to extract text from file: {ex.Message}");
            }

            if (string.IsNullOrWhiteSpace(rawText))
            {
                return Result<List<ScanQuestionDto>>.Failure("The file contains no text.");
            }

            string rolePrompt = "You are a fast, automated data extraction tool. You must not use <think> tags or output any reasoning.";
            string requirement = @"Extract multiple-choice questions from the text below. 
DO NOT output <think> blocks. DO NOT provide any reasoning, explanation, or markdown wrappers. 
OUTPUT STRICTLY A MINIFIED JSON ARRAY.
Schema: [{""StringContent"":"""",""Explanation"":"""",""Level"":1,""CategoryIds"":[],""Answers"":[{""StringContent"":"""",""IsCorrectAnswer"":true}]}]

RAW TEXT:
" + rawText;

            try
            {
                var aiResponse = await _aiService.PromptAndAsk(rolePrompt, requirement);
                
                // Sometimes AI still wraps in markdown despite instructions. Let's clean it up.
                aiResponse = aiResponse.Trim();
                if (aiResponse.StartsWith("```json"))
                {
                    aiResponse = aiResponse.Substring(7);
                    if (aiResponse.EndsWith("```"))
                    {
                        aiResponse = aiResponse.Substring(0, aiResponse.Length - 3);
                    }
                }
                else if (aiResponse.StartsWith("```"))
                {
                    aiResponse = aiResponse.Substring(3);
                    if (aiResponse.EndsWith("```"))
                    {
                        aiResponse = aiResponse.Substring(0, aiResponse.Length - 3);
                    }
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var questions = JsonSerializer.Deserialize<List<ScanQuestionDto>>(aiResponse, options);

                return Result<List<ScanQuestionDto>>.Success("Scanned successfully.", questions ?? new List<ScanQuestionDto>());
            }
            catch (Exception ex)
            {
                return Result<List<ScanQuestionDto>>.Failure($"Failed to parse AI response: {ex.Message}");
            }
        }
    }
}
