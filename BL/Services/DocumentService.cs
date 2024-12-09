using AutoMapper;
using DAL.Persistence;
using SharedResources.DTO;
using SharedResources.Entities;
using RabbitMq.QueueLibrary;
using System;
using System.Collections.Generic;
using BL.Validators;
using FileStorageService.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ElasticSearch;

namespace BL.Services
{
    public class DocumentService : IDocumentServices
    {
        private readonly IDocumentRepo _documentRepo;
        private readonly IFilesApi _filesApi;
        private readonly IMapper _mapper;
        private readonly IQueueProducer _queueProducer;
        private readonly IQueueConsumer _queueConsumer;
        private readonly ISearchIndex _searchIndex;
        public DocumentService(IDocumentRepo documentRepo, IMapper mapper, IQueueProducer queueProducer, IQueueConsumer queueConsumer, IFilesApi filesApi, ISearchIndex searchIndex)
        {
            _documentRepo = documentRepo;
            _mapper = mapper;
            _queueProducer = queueProducer;
            _queueConsumer = queueConsumer;
            _filesApi = filesApi;
            _searchIndex = searchIndex;
        }

        private async Task<string> ConsumeOcrQueue()
        {
            _queueConsumer.StartReceive();

            var taskCompletionSource = new TaskCompletionSource<string>();

            _queueConsumer.OnReceived += (sender, eventArgs) =>
            {
                Console.WriteLine("Message received in event handler.");
                var content = eventArgs.Content;
                
                taskCompletionSource.SetResult(content);
                _queueConsumer.OnReceived -= null;



            };


            try
            {
                var result = await taskCompletionSource.Task;

                return result;
            }
            catch (Exception ex)
            {
                // Log the error or handle it as needed
                throw new Exception("Error while waiting for the first message", ex);
            }

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

        

        public async Task<string> CreateDocument(DocumentBl documentDto, Stream fileStream, string contentType)
        {
            var validator = new DocumentValidator();
            var results = validator.Validate(documentDto);

            if (!results.IsValid)
            {
                throw new ValidationException(results.Errors);
            }

            await _filesApi.UploadAsync(fileStream, documentDto.Filepath, contentType);

            var documentDal = _mapper.Map<DocumentDAL>(documentDto);
            _documentRepo.Create(documentDal);

            SendToQueue(documentDto.Filepath, documentDto.Id);

            var result = await ConsumeOcrQueue();

            return result;
        }

        public List<DocumentBl> GetDocuments()
        {
            var documents = _documentRepo.Get();

            return _mapper.Map<List<DocumentBl>>(documents);
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

        public void SendToQueue(string filePath, Guid documentId)
        {
            _queueProducer.Send(filePath, documentId);
        }

        public List<DocumentBl> SearchDocuments(string query)
        {
            return _mapper.Map < List < DocumentBl >> (_searchIndex.SearchDocumentAsync(query).Select(doc => _documentRepo.Read(doc.Id)).ToList());

        }
    }
}
