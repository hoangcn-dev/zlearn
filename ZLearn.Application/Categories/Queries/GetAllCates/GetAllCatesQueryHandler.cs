using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZLearn.Application.Categories.DTOs;
using ZLearn.Application.Categories.Queries.GetPaginatedCate;

namespace ZLearn.Application.Categories.Queries.GetAllCates
{
    public class GetAllCatesQueryHandler : IRequestHandler<GetAllCatesQuery, IEnumerable<CateListItemDto>>
    {
        private readonly ICateRepo _repo;
        private readonly IMapper _mapper;

        public GetAllCatesQueryHandler(ICateRepo repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CateListItemDto>> Handle(GetAllCatesQuery request, CancellationToken cancellationToken)
        {
            var cates = await _repo.GetAll();
            return cates.Select(c => _mapper.Map<CateListItemDto>(c));
        }
    }
}
