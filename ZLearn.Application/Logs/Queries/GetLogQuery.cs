using ZLearn.Application.Logs.DTOs;

namespace ZLearn.Application.Logs.Queries
{
    public class GetLogQuery : IRequest<List<LogListItemDto>>
    {
        public DateTime? Date { get; set; }
    }
}
