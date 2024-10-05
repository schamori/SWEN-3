using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sws.Models;
using System.Threading.Tasks;


namespace WebApplicationSWEN3.Controllers
{
    [ApiController]
    [Route("/api/document")]
    public class DocumentController : Controller
    {
        private readonly DocumentContext _context;

        public DocumentController(DocumentContext context)
        {
            _context = context;
        }

        // GET: /Document
        [HttpGet]
        public IActionResult Get()
        {
            if (_context.DocumentItems.Count() == 0)
            {
                // Hardcode 5 documents if no documents found
                var documents = new List<DocumentItem>
                {
                    new DocumentItem { Id = 1, Title = "Document 1", Content = "Lorem ipsum dolor sit amet." },
                    new DocumentItem { Id = 2, Title = "Document 2", Content = "Consectetur adipiscing elit." },
                    new DocumentItem { Id = 3, Title = "Document 3", Content = "Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua." },
                    new DocumentItem { Id = 4, Title = "Document 4", Content = "Ut enim ad minim veniam." },
                    new DocumentItem { Id = 5, Title = "Document 5", Content = "Quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat." }
                };

                return Ok(documents);
            }
            return Ok(_context.DocumentItems);
        }


        // GET: /Document/{id}

        [HttpGet("{id}")]
        public IActionResult GetDocument(int id)
        {
            var document = _context.DocumentItems.Find(id);
            if (document == null)
            {
                return NotFound($"Document with Id {id} not found!");
            }
            return Ok(document);
        }

        // Post: /Document
        [HttpPost]
        public async Task<ActionResult<DocumentItem>> PostDocument(DocumentItem documentItem)
        {
            _context.DocumentItems.Add(documentItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetDocumentItem", new { id = documentItem.Id }, documentItem);
        }

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
    }
}