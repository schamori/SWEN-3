using AutoMapper;
using DAL.Persistence;
using SharedResources.DTO;
using SharedResources.Entities;
using System;
using System.Collections.Generic;

namespace BL.Services
{
    public class DocumentService
    {
        private readonly IDocumentRepo _documentRepo;
        private readonly IMapper _mapper;

        public DocumentService(IDocumentRepo documentRepo, IMapper mapper)
        {
            _documentRepo = documentRepo;
            _mapper = mapper; 
        }

        public DocumentDTO GetDocumentById(Guid id)
        {
            var documentDal = _documentRepo.Read(id);
            if (documentDal == null)
            {
                return null;
            }

            return _mapper.Map<DocumentDTO>(documentDal);
        }

        public DocumentDTO CreateDocument(DocumentDTO documentDto)
        {
            if (string.IsNullOrEmpty(documentDto.Title))
            {
                throw new ArgumentException("Title is required.");
            }

            var documentDal = _mapper.Map<DocumentDAL>(documentDto);

            _documentRepo.Create(documentDal);

            return documentDto;
        }

        public List<DocumentDTO> GetDocuments()
        {
            var documents = _documentRepo.Get();

            return _mapper.Map<List<DocumentDTO>>(documents);
        }

        public DocumentDTO UpdateDocument(DocumentDTO documentDto)
        {
            var documentDal = _documentRepo.Read(documentDto.Id);

            if (documentDal == null)
            {
                return null;
            }

            // Verwende den Mapper, um das DTO auf das bestehende DAL-Objekt zu mappen
            _mapper.Map(documentDto, documentDal);

            _documentRepo.Update(documentDal);

            return documentDto;
        }

        public bool DeleteDocument(Guid id)
        {
            var document = _documentRepo.Read(id);
            if (document == null)
            {
                return false;
            }

            _documentRepo.Delete(id);
            return true;
        }
    }
}
