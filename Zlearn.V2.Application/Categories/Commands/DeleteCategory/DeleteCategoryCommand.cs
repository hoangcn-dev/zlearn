using System.Collections.Generic;
using MediatR;

namespace Zlearn.V2.Application.Categories.Commands.DeleteCategory
{
    public record DeleteCategoryCommand(List<string> Ids) : IRequest<bool>;
}
