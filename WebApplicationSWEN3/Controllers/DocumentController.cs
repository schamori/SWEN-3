using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using DAL.DTO;
using DAL.Persistence;



namespace WebApplicationSWEN3.Controllers
{
    [ApiController]
    [Route("/api/document")]

    public class DocumentController : Controller
    {
        private readonly DocumentRepo _documentRepo;

        public DocumentController(DocumentRepo context)
        {
            _documentRepo = context;
        }

        // GET: /Document
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_documentRepo.Get());
        }


        // GET: /Document/{id}

        [HttpGet("{id}")]
        public IActionResult GetDocument(int id)
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
        public async Task<ActionResult<DocumentDTO>> PostDocument([FromBody] CreateDocumentDTO documentItem)
        {
            var createdDocument = _documentRepo.Create(documentItem);

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