using ZLearn.Application.Logs.DTOs;

namespace ZLearn.Application.Logs
{
    public interface ILogService
    {
        Task<List<LogListItemDto>> GetLogsOfDay(DateTime? date);
    }
}
