using AutoMapper;
using DAL.Persistence;
using SharedResources.DTO;
using SharedResources.Entities;
using RabbitMq.QueueLibrary;
using System;
using System.Collections.Generic;
using BL.Validators;

namespace BL.Services
{
    public class DocumentService : IDocumentServices
    {
        private readonly IDocumentRepo _documentRepo;
        private readonly IMapper _mapper;
        private readonly IQueueProducer _queueProducer;

        public DocumentService(IDocumentRepo documentRepo, IMapper mapper, IQueueProducer queueProducer)
        {
            _documentRepo = documentRepo;
            _mapper = mapper;
            _queueProducer = queueProducer;
        }

        public DocumentBl GetDocumentById(Guid id)
        {
            var documentDal = _documentRepo.Read(id);
            if (documentDal == null)
            {
                return null;
            }
            return _mapper.Map<DocumentBl>(documentDal);
        }

        public DocumentBl CreateDocument(DocumentBl documentDto)
        {
            var validator = new DocumentValidator();
            var results = validator.Validate(documentDto);

            if (!results.IsValid)
            {
                throw new ValidationException(results.Errors);
            }

            var documentDal = _mapper.Map<DocumentDAL>(documentDto);
            _documentRepo.Create(documentDal);

            SendToQueue(documentDto.Filepath, documentDto.Id);

            return _mapper.Map<DocumentBl>(documentDal);
        }

        public List<DocumentBl> GetDocuments()
        {
            var documents = _documentRepo.Get();

            return _mapper.Map<List<DocumentBl>>(documents);
        }

        /*public DocumentDTO UpdateDocument(DocumentBl documentDto)
        {
            var documentDal = _documentRepo.Read(documentDto.Id);

            if (documentDal == null)
            {
                return null;
            }

            _mapper.Map(documentDto, documentDal);

            _documentRepo.Update(documentDal);

            return documentDto;
        }*/

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

        public void SendToQueue(string filePath, Guid documentId)
        {
            _queueProducer.Send(filePath, documentId);
        }
    }
}
