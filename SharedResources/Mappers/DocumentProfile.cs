using AutoMapper;
using SharedResources.Entities;
using SharedResources.DTO;

namespace SharedResources.Mappers
{
    public class DocumentProfile : Profile
    {
        public DocumentProfile()
        {
            CreateMap<DocumentDTO, DocumentBl>().ReverseMap();
            CreateMap<DocumentBl, DocumentDAL>().ReverseMap();

        }
    }
}
