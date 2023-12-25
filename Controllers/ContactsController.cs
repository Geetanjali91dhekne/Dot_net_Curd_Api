using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPIs.Model;
using WebAPIs.data;

namespace WebAPIs.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactsController : ControllerBase
    {
        private readonly ContactsAPIDbContext _context;

        public ContactsController(ContactsAPIDbContext context)
        {
            _context = context;
        }

        // GET: api/Contacts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Contacts>>> GetContacts()
        {
          if (_context.Contacts == null)
          {
              return NotFound();
          }
            return await _context.Contacts.ToListAsync();
        }

        // GET: api/Contacts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Contacts>> GetContacts(Guid id)
        {
          if (_context.Contacts == null)
          {
              return NotFound();
          }
            var contacts = await _context.Contacts.FindAsync(id);

            if (contacts == null)
            {
                return NotFound();
            }

            return contacts;
        }

        // PUT: api/Contacts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutContacts(Guid id, Contacts contacts)
        {
            if (id != contacts.id)
            {
                return BadRequest();
            }

            _context.Entry(contacts).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ContactsExists(id))
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

        // POST: api/Contacts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Contacts>> PostContacts(Contacts contacts)
        {
          if (_context.Contacts == null)
          {
              return Problem("Entity set 'ContactsAPIDbContext.Contacts'  is null.");
          }
            _context.Contacts.Add(contacts);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetContacts", new { id = contacts.id }, contacts);
        }

        // DELETE: api/Contacts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContacts(Guid id)
        {
            if (_context.Contacts == null)
            {
                return NotFound();
            }
            var contacts = await _context.Contacts.FindAsync(id);
            if (contacts == null)
            {
                return NotFound();
            }

            _context.Contacts.Remove(contacts);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ContactsExists(Guid id)
        {
            return (_context.Contacts?.Any(e => e.id == id)).GetValueOrDefault();
        }
    }
}
