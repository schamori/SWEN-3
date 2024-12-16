using AutoMapper;
using DAL.Persistence;
using SharedResources.DTO;
using SharedResources.Entities;
using RabbitMq.QueueLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BL.Validators;
using FileStorageService.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using ElasticSearch;
using BL.Exceptions;
using DAL.Exceptions;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<DocumentService> _logger;

        public DocumentService(IDocumentRepo documentRepo, IMapper mapper, IQueueProducer queueProducer,
                               IQueueConsumer queueConsumer, IFilesApi filesApi, ISearchIndex searchIndex,
                               ILogger<DocumentService> logger)
        {
            _documentRepo = documentRepo;
            _mapper = mapper;
            _queueProducer = queueProducer;
            _queueConsumer = queueConsumer;
            _filesApi = filesApi;
            _searchIndex = searchIndex;
            _logger = logger;
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
                _logger.LogError(ex, "Error while waiting for the first message.");
                throw new BusinessLogicException("Fehler beim Warten auf die erste Nachricht.", ex);
            }
        }

        public DocumentBl GetDocumentById(Guid id)
        {
            try
            {
                var documentDal = _documentRepo.Read(id);
                if (documentDal == null)
                {
                    _logger.LogWarning($"Document with ID: {id} not found.");
                    return null;
                }
                return _mapper.Map<DocumentBl>(documentDal);
            }
            catch (DataAccessException dae)
            {
                _logger.LogError(dae, $"Data access error while fetching document with ID: {id}.");
                throw new BusinessLogicException($"Fehler beim Abrufen des Dokuments mit ID: {id}.", dae);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while fetching document with ID: {id}.");
                throw new BusinessLogicException($"Unerwarteter Fehler beim Abrufen des Dokuments mit ID: {id}.", ex);
            }
        }

        public async Task<string> CreateDocument(DocumentBl documentDto, Stream fileStream, string contentType)
        {
            try
            {
                var validator = new DocumentValidator();
                var results = validator.Validate(documentDto);

                if (!results.IsValid)
                {
                    throw new ValidationException(results.Errors);
                }

                await _filesApi.UploadAsync(fileStream, documentDto.Id.ToString(), contentType);

                var documentDal = _mapper.Map<DocumentDAL>(documentDto);
                _documentRepo.Create(documentDal);

                SendToQueue(documentDto.Filepath, documentDto.Id);

                var result = await ConsumeOcrQueue();

                return result;
            }
            catch (ValidationException vex)
            {
                _logger.LogError(vex, "Validation failed for the document.");
                throw new BusinessLogicException("Validierung des Dokuments fehlgeschlagen.", vex);
            }
            catch (DataAccessException dae)
            {
                _logger.LogError(dae, $"Data access error while creating document with ID: {documentDto.Id}.");
                throw new BusinessLogicException($"Fehler beim Erstellen des Dokuments mit ID: {documentDto.Id}.", dae);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while creating document with ID: {documentDto.Id}.");
                throw new BusinessLogicException($"Unerwarteter Fehler beim Erstellen des Dokuments mit ID: {documentDto.Id}.", ex);
            }
        }

        public List<DocumentBl> GetDocuments()
        {
            try
            {
                var documents = _documentRepo.Get();
                return _mapper.Map<List<DocumentBl>>(documents);
            }
            catch (DataAccessException dae)
            {
                _logger.LogError(dae, "Data access error while fetching documents.");
                throw new BusinessLogicException("Fehler beim Abrufen der Dokumente.", dae);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while fetching documents.");
                throw new BusinessLogicException("Unerwarteter Fehler beim Abrufen der Dokumente.", ex);
            }
        }

        public bool DeleteDocument(Guid id)
        {
            try
            {
                var document = _documentRepo.Read(id);
                if (document == null)
                {
                    _logger.LogWarning($"Document with ID: {id} not found for deletion.");
                    return false;
                }

                _documentRepo.Delete(id);
                return true;
            }
            catch (DataAccessException dae)
            {
                _logger.LogError(dae, $"Data access error while deleting document with ID: {id}.");
                throw new BusinessLogicException($"Fehler beim Löschen des Dokuments mit ID: {id}.", dae);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An unexpected error occurred while deleting document with ID: {id}.");
                throw new BusinessLogicException($"Unerwarteter Fehler beim Löschen des Dokuments mit ID: {id}.", ex);
            }
        }

        public void SendToQueue(string filePath, Guid documentId)
        {
            try
            {
                _queueProducer.Send(filePath, documentId);
                _logger.LogInformation($"Extracted text for {filePath} with MessageId: {documentId} sent to producer.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while sending extracted text to the queue for document ID: {documentId}.");
                throw new BusinessLogicException($"Fehler beim Senden des extrahierten Textes an die Queue für Dokument ID: {documentId}.", ex);
            }
        }

        public List<DocumentBl> SearchDocuments(string query)
        {
            try
            {
                var searchResults = _searchIndex.SearchDocumentAsync(query);
                var documents = searchResults.Select(doc => _documentRepo.Read(doc.Id)).Where(d => d != null).ToList();
                return _mapper.Map<List<DocumentBl>>(documents);
            }
            catch (DataAccessException dae)
            {
                _logger.LogError(dae, "Data access error while searching documents.");
                throw new BusinessLogicException("Fehler bei der Dokumentensuche.", dae);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while searching documents.");
                throw new BusinessLogicException("Unerwarteter Fehler bei der Dokumentensuche.", ex);
            }
        }

        public async Task<byte[]> GetDocumentFile(Guid id)
        {
            try
            {
                return await _filesApi.DownloadFromMinioAsync("documents", id.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while downloading the file for document ID: {id}.");
                throw new BusinessLogicException($"Fehler beim Herunterladen der Datei für Dokument ID: {id}.", ex);
            }
        }
    }
}
