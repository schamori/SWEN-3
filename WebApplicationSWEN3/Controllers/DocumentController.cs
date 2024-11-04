using Microsoft.AspNetCore.Mvc;
using BL.Services;
using SharedResources.DTO;
using System;
using System.Threading.Tasks;
using RabbitMq.QueueLibrary;

namespace WebApplicationSWEN3.Controllers
{
    [ApiController]
    [Route("/api/document")]
    public class DocumentController : Controller
    {
        private readonly DocumentService _documentService;
        private readonly IQueueProducer _queueProducer;  // Füge den QueueProducer hinzu

        public DocumentController(DocumentService documentService, IQueueProducer queueProducer)
        {
            _documentService = documentService;
            _queueProducer = queueProducer;  // Weist den QueueProducer zu
        }

        // GET: /Document
        [HttpGet]
        public IActionResult Get()
        {
            var documents = _documentService.GetDocuments();
            return Ok(documents);
        }

        // GET: /Document/{id}
        [HttpGet("{id}")]
        public IActionResult GetDocument(Guid id)
        {
            var document = _documentService.GetDocumentById(id);
            if (document == null)
            {
                return NotFound($"Document with Id {id} not found!");
            }
            return Ok(document);
        }

        // POST: /Document
        [HttpPost]
        public async Task<ActionResult<DocumentDTO>> PostDocument([FromForm] IFormFile file)
        {
            var documentDto = new DocumentDTO
            {
                Id = Guid.NewGuid(),
                Title = file.FileName,
                Filepath = file.FileName
            };

            var result = _documentService.CreateDocument(documentDto);

            if (result == null)
            {
                return BadRequest("Validation failed or document could not be created.");
            }

            // Schicke eine Nachricht an die Queue, nachdem das Dokument erstellt wurde
            _queueProducer.Send(result.Title, result.Id);

            return CreatedAtAction(nameof(GetDocument), new { id = result.Id }, result);
        }

        /*
        // DELETE: /Document/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteDocument(Guid id)
        {
            var result = _documentService.DeleteDocument(id);

            if (!result)
            {
                return NotFound($"Document with Id {id} not found!");
            }

            // Nachricht an die Queue senden
            _queueProducer.Produce($"Document with ID {id} has been deleted.");

            return NoContent();
        }

        // PUT: /Document/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateDocument(Guid id, [FromBody] DocumentDTO documentDto)
        {
            if (id != documentDto.Id)
            {
                return BadRequest("Document ID mismatch.");
            }

            var updatedDocument = _documentService.UpdateDocument(documentDto);

            if (updatedDocument == null)
            {
                return NotFound($"Document with Id {id} not found!");
            }

            // Nachricht an die Queue senden
            _queueProducer.Produce($"Document {updatedDocument.Title} with ID {updatedDocument.Id} has been updated.");

            return NoContent();
        }*/
    }
}
