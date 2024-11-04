using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using DAL.Persistence;
using SharedResources.Entities;
using AutoMapper;
using SharedResources.Validators;
using RabbitMq.QueueLibrary;
using SharedResources.DTO;

namespace WebApplicationSWEN3.Controllers
{
    [ApiController]
    [Route("/api/document")]

    public class DocumentController : Controller
    {
        private readonly IMapper _mapper;

        private readonly IDocumentRepo _documentRepo;
        private readonly IQueueProducer _queueProducer;
        public DocumentController(IDocumentRepo context, IMapper mapper, IQueueProducer queueProducer)
        {
            _documentRepo = context;
            _mapper = mapper;
            _queueProducer = queueProducer;

        }

        // GET: /Document
        [HttpGet]
        public IActionResult Get()
        {
            List<DocumentDAL> documentDalList = _documentRepo.Get();

            List<DocumentBl> documentBlList = _mapper.Map<List<DocumentBl>>(documentDalList);

            List<DocumentDTO> documentDtoList = _mapper.Map<List<DocumentDTO>>(documentBlList);

            return Ok(documentDtoList);
        }


        // GET: /Document/{id}

        [HttpGet("{id}")]
        public IActionResult GetDocument(Guid id)
        {
            var document = _documentRepo.Read(id);
            if (document == null)
            {
                return NotFound($"Document with Id {id} not found!");
            }
            return Ok(document);
        }

        // Post: /Document
        [HttpPost]
        public async Task<ActionResult<DocumentDTO>> PostDocument([FromForm] IFormFile file)
        {
            DocumentBl documentItem = new DocumentBl
            {
                Id = Guid.NewGuid(),
                Title = file.FileName,
                Filepath = file.FileName
            };

            _queueProducer.Send(documentItem.Filepath, documentItem.Id);


            var validator = new DocumentValidator();
            var results = validator.Validate(documentItem);

            if (!results.IsValid)
            {
                foreach (var failure in results.Errors)
                {
                    Console.WriteLine("Property " + failure.PropertyName + " failed validation. Error was: " + failure.ErrorMessage);
                }
                return StatusCode(StatusCode(400).StatusCode, results.Errors);
            }



            var createdDocument = _documentRepo.Create(_mapper.Map<DocumentDAL>(documentItem));

            return CreatedAtAction(nameof(GetDocument), new { id = createdDocument.Id }, createdDocument);
        }

        /*
        //  Delete: Document/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            var document = await _context.DocumentItems.FindAsync(id);
            if (document == null)
            {
                return NotFound("No Documents saved!");
            }

            _context.DocumentItems.Remove(document);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // PUT: Document/{id}

        [HttpPut("{id}")]
        public async Task<IActionResult> PutDocument(int id, DocumentItem documentItem)
        {
            if (id != documentItem.Id)
            {
                return BadRequest();
            }

            _context.Entry(documentItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DocumentExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool DocumentExists(int id)
        {
            return _context.DocumentItems.Any(e => e.Id == id);
        }

        */
    }
}