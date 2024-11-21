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
                
                var createdDocument = _bl.CreateDocument(_mapper.Map<DocumentBl>(documentItem));

                _logger.LogInformation($"Document created successfully with ID: {createdDocument.Id}");

                return CreatedAtAction(nameof(GetDocument), new { id = createdDocument.Id }, createdDocument);
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

        /* 
        // DELETE: /Document/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(Guid id)
        {
            try
            {
                _logger.LogInformation($"Deleting document with ID: {id}");
                var document = _documentRepo.Read(id);

                if (document == null)
                {
                    _logger.LogWarning($"Document with ID: {id} not found for deletion.");
                    return NotFound("Document not found.");
                }

                _documentRepo.Delete(id);
                _logger.LogInformation($"Document with ID: {id} deleted successfully.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while deleting document with ID: {id}");
                return StatusCode(500, "Internal server error.");
            }
        }

        // PUT: /Document/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDocument(Guid id, [FromBody] DocumentDTO documentDto)
        {
            try
            {
                _logger.LogInformation($"Updating document with ID: {id}");

                if (id != documentDto.Id)
                {
                    _logger.LogWarning($"Document ID in the URL ({id}) does not match the document ID in the body ({documentDto.Id}).");
                    return BadRequest("Document ID mismatch.");
                }

                var documentBl = _mapper.Map<DocumentBl>(documentDto);
                var updatedDocument = _documentRepo.Update(_mapper.Map<DocumentDAL>(documentBl));

                if (updatedDocument == null)
                {
                    _logger.LogWarning($"Document with ID: {id} not found for update.");
                    return NotFound("Document not found.");
                }

                _logger.LogInformation($"Document with ID: {id} updated successfully.");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"An error occurred while updating document with ID: {id}");
                return StatusCode(500, "Internal server error.");
            }
        }
        */
    }
}
