namespace ZLearn.Application.Common.Services
{
    public interface IAIService
    {
        Task<string> PromptAndAsk(string rolePrompt, string requirement);
    }
}
