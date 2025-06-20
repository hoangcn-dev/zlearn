using AutoMapper;
using ZLearn.Domain.Entities;

namespace ZLearn.Application.Categories.DTOs
{
    public class CateListItemDto
    {
        public string Id { get; set; }
        public string Name { get; set; }

        private class Mapping : Profile
        {
            public Mapping()
            {
                CreateMap<Category, CateListItemDto>();
            }
        }
    }
}
