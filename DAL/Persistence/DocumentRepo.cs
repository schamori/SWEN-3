using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedResources.Entities;

namespace DAL.Persistence
{
    public class DocumentRepo : IDocumentRepo
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DocumentRepo> _logger;

        public DocumentRepo(ApplicationDbContext context, IMapper mapper, ILogger<DocumentRepo> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<DocumentDAL> Get()
        {
            try
            {
                _logger.LogInformation("Fetching all documents.");
                return _context.Documents.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching documents.");
                throw;
            }
        }

        public DocumentDAL Create(DocumentDAL document)
        {
            try
            {
                document.CreatedAt = DateTime.UtcNow;
                document.UpdatedAt = DateTime.UtcNow;

                _context.Documents.Add(document);
                _context.SaveChanges();

                _logger.LogInformation($"Document created successfully with ID: {document.Id}");
                return document;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the document.");
                throw;
            }
        }

        public DocumentDAL Read(Guid id)
        {
            try
            {
                _logger.LogInformation($"Fetching document with ID: {id}");
                var documentEntity = _context.Documents.FirstOrDefault(d => d.Id == id);

                if (documentEntity == null)
                {
                    _logger.LogWarning($"Document with ID: {id} not found.");
                    return null;
                }

                return documentEntity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching the document with ID: {id}");
                throw;
            }
        }

        public DocumentDAL Update(DocumentDAL document)
        {
            try
            {
                var foundDocument = _context.Documents.FirstOrDefault(d => d.Id == document.Id);

                if (foundDocument != null)
                {
                    foundDocument.Title = document.Title;
                    foundDocument.Filepath = document.Filepath;
                    _context.Entry(foundDocument).State = EntityState.Modified;
                    _context.SaveChanges();

                    _logger.LogInformation($"Document updated successfully with ID: {document.Id}");
                    return foundDocument;
                }

                _logger.LogWarning($"Document with ID: {document.Id} not found for update.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating the document with ID: {document.Id}");
                throw;
            }
        }

        public void Delete(Guid id)
        {
            try
            {
                var documentEntity = _context.Documents.FirstOrDefault(d => d.Id == id);

                if (documentEntity != null)
                {
                    _context.Documents.Remove(documentEntity);
                    _context.SaveChanges();
                    _logger.LogInformation($"Document deleted successfully with ID: {id}");
                }
                else
                {
                    _logger.LogWarning($"Document with ID: {id} not found for deletion.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting the document with ID: {id}");
                throw;
            }
        }
    }
}
