using AutoMapper;
using DAL.DTO;
using DAL.Entities;
using SharedResources.Entities;

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
