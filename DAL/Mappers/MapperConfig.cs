using DAL.DTO;
using DAL.Entities;
using AutoMapper;


namespace DAL.Mappers
{
    public class MapperConfig
    {
        public static Mapper InitializeAutomapper()
        {
            //Provide all the Mapping Configuration
            var config = new MapperConfiguration(cfg =>
            {

                cfg.CreateMap<Document, DocumentDTO>();
                cfg.CreateMap<DocumentDTO, Document>();

              });

            //Create an Instance of Mapper and return that Instance
            var mapper = new Mapper(config);
            return mapper;
        }
    }

}

