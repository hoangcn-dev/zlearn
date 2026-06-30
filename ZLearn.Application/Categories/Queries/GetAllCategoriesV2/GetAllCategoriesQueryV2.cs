using System.Collections.Generic;
using MediatR;
using ZLearn.Application.Categories.DTOs;

namespace ZLearn.Application.Categories.Queries.GetAllCategoriesV2
{
    public class GetAllCategoriesQueryV2 : IRequest<IEnumerable<CategoryDocument>>
    {
    }
}
