using AutoMapper;
using DAL.DTO;
using DAL.Entities;

namespace DAL.Mappers
{
    public class DocumentProfile : Profile
    {
        public DocumentProfile()
        {
            // Mapping Document -> DocumentDTO und umgekehrt
            CreateMap<Document, DocumentDTO>();
            CreateMap<DocumentDTO, Document>();
            CreateMap<DocumentDTO, Document>().ReverseMap();

            // Mapping CreateDocumentDTO -> Document
            CreateMap<CreateDocumentDTO, Document>()
                .ForMember(dest => dest.Id, opt => opt.Ignore()) 
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.Now));
        }
    }
}
