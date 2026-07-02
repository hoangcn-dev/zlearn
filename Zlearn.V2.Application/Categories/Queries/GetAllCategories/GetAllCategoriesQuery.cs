using System.Collections.Generic;
using MediatR;
using Zlearn.V2.Application.Categories.DTOs;

namespace Zlearn.V2.Application.Categories.Queries.GetAllCategories
{
    public class GetAllCategoriesQuery : IRequest<IEnumerable<CateListItemDto>>
    {
    }
}


