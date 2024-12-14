using Microsoft.AspNetCore.Mvc;
using DAL.Persistence;
using SharedResources.Entities;
using AutoMapper;
using BL.Validators;
using BL.Services;
using RabbitMq.QueueLibrary;
using SharedResources.DTO;
using FluentValidation;
using BL;
using ElasticSearch;

namespace WebApplicationSWEN3.Controllers
{
    [ApiController]
    [Route("/api/document")]
    public class DocumentController : Controller
    {
        private readonly IMapper _mapper;
        private readonly IDocumentServices _bl;
        private readonly IQueueProducer _queueProducer;
        private readonly ILogger<DocumentController> _logger;

        public DocumentController( IMapper mapper, IQueueProducer queueProducer, 
            ILogger<DocumentController> logger, IDocumentServices bl)
        {
            _bl = bl;
            _mapper = mapper;
            _queueProducer = queueProducer;
            _logger = logger;
            
        }

        [HttpGet("Search")]

        public IActionResult SearchDocuments([FromQuery] string? query)
        {
            try
            {
                _logger.LogInformation("Searching for documents with query parameters.");

                // Example: Filter documents based on the provided query parameters.
                var documents = _bl.SearchDocuments(query!);


                if (!documents.Any())
                {
                    _logger.LogWarning("No documents found matching the search criteria.");
                    return NotFound("No documents found matching the search criteria.");
                }

                return Ok(documents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while searching for documents.");
                return StatusCode(500, "Internal server error.");
            }
        }

        // GET: /Document
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                _logger.LogInformation("Fetching all documents.");
                var documentDalList = _bl.GetDocuments();

                return Ok(documentDalList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all documents.");
                return StatusCode(500, "Internal server error.");
            }
        }

        // GET: /Document/{id}
        [HttpGet("{id}")]
        public IActionResult GetDocument(Guid id)
        {
            try
            {
                _logger.LogInformation($"Fetching document with ID: {id}");
                var document = _bl.GetDocumentById(id);
                if (document == null)
                {
                    _logger.LogWarning($"Document with ID: {id} not found.");
                    return NotFound($"Document with ID {id} not found!");
                }
                return Ok(document);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while fetching document with ID: {id}");
                return StatusCode(500, "Internal server error.");
            }
        }
        
        // POST: /Document
        [HttpPost]
        public async Task<ActionResult<DocumentDTO>> PostDocument([FromForm] IFormFile file)
        {
            try
            {
                _logger.LogInformation("Creating a new document.");

                var documentItem = new DocumentDTO
                {
                    Id = Guid.NewGuid(),
                    Title = file.FileName,
                    Filepath = file.FileName
                };
                using var fileStream = file.OpenReadStream();
                var documentOcr = await _bl.CreateDocument(_mapper.Map<DocumentBl>(documentItem),fileStream,file.ContentType);



                _logger.LogInformation($"Document Ocr received: {documentOcr}");

                return CreatedAtAction(nameof(GetDocument), new { id = documentItem.Id }, documentItem);
            }
            catch (BL.ValidationException ex)
            {
                _logger.LogWarning("Document validation failed.");
                return BadRequest(ex.Errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a new document.");
                return StatusCode(500, "Internal server error.");
            }
        }
        [HttpGet("download/{document_id}")]
        public async Task<ActionResult> DownloadDocument(Guid document_id)
        {
            try
            {
                var document = _bl.GetDocumentById(document_id);
                if (document == null)
                {
                    _logger.LogWarning($"Document with ID: {document_id} not found.");
                    return NotFound($"Document with ID {document_id} not found!");
                }
                _logger.LogInformation($"Downloading document with ID: {document_id}");
                var documentFile = await _bl.GetDocumentFile(document_id);
                if (documentFile == null)
                {
                    _logger.LogWarning($"Document with ID: {document_id} not found.");
                    return NotFound($"Document with ID {document_id} not found!");
                }

                var contentType = "application/pdf";

                return File(documentFile, contentType, document.Filepath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while downloading document with ID: {document_id}");
                return StatusCode(500, "Internal server error.");
            }
        }


    }
}
