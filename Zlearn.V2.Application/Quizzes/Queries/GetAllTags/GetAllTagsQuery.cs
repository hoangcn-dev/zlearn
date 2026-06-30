using System.Collections.Generic;
using MediatR;

namespace Zlearn.V2.Application.Quizzes.Queries.GetAllTags
{
    public class GetAllTagsQuery : IRequest<IEnumerable<string>>
    {
    }
}
