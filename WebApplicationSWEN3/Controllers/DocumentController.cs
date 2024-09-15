using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sws.Models;
using System.Threading.Tasks;


namespace WebApplicationSWEN3.Controllers
{
    [ApiController]
    [Route("/Document")]
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
                return NotFound();
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
                return NotFound();
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