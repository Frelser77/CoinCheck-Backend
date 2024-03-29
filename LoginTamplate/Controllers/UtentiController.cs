using LoginTamplate.Model;
using LoginTamplate.Model.Dto.Utente;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoginTamplate.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UtentiController : ControllerBase
    {
        private readonly CoinCheckContext _context;
        private readonly string _profileImagesPath;

        public UtentiController(CoinCheckContext context, IWebHostEnvironment env)
        {
            _context = context;
            _profileImagesPath = Path.Combine(env.ContentRootPath, "uploads", "profile");

            if (!Directory.Exists(_profileImagesPath))
            {
                Directory.CreateDirectory(_profileImagesPath);
            }
        }

        // GET: api/Utenti
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UtenteDto>>> GetUtenti()
        {
            var utentiDtoList = await _context.Utentis
                .Select(u => new UtenteDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    IsActive = u.IsActive,
                    ImageUrl = u.ImageUrl,
                    Comments = u.Comments.Select(c => c.Content), // Assumi che Content sia una stringa da mostrare
                    LogAttivita = u.LogAttivita.Select(la => la.Timestamp.ToString()), // Assumi che Azione sia una stringa da mostrare
                    Posts = u.Posts.Select(p => p.Title), // Assumi che Title sia una stringa da mostrare
                    PreferenzeUtentes = u.PreferenzeUtentes.Select(pu => new PreferenzeUtenteDto
                    {
                        // Mappa le proprietà necessarie qui
                    }).ToList()
                })
                .ToListAsync();

            return utentiDtoList;
        }

        // GET: api/Utenti/5
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,Moderatore,Utente")]
        public async Task<ActionResult<UtenteDto>> GetUtente(int id)
        {
            var utente = await _context.Utentis
                .Where(u => u.UserId == id)
                .Select(u => new UtenteDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    IsActive = u.IsActive,
                    ImageUrl = u.ImageUrl,
                    LogAttivita = u.LogAttivita.Select(l => l.Timestamp.ToString()),
                    Posts = u.Posts.Select(p => p.Title),
                    Comments = u.Comments.Select(c => c.Content),
                    PreferenzeUtentes = u.PreferenzeUtentes
                        .Select(p => new PreferenzeUtenteDto
                        {
                            CriptoId = p.CriptoId,
                            NotificaSogliaPrezzo = p.NotificaSogliaPrezzo,
                            SogliaPrezzo = p.SogliaPrezzo
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            if (utente == null)
            {
                return NotFound();
            }

            return utente;
        }

        // PUT: api/Utenti/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Moderatore,Utente")]
        public async Task<IActionResult> UpdateUtente(int id, [FromBody] UtentiPutDto utenteDto)
        {
            var utenteToUpdate = await _context.Utentis.FindAsync(id);
            if (utenteToUpdate == null)
            {
                return NotFound($"Utente con ID {id} non trovato.");
            }

            // Aggiorna solo se il valore nel DTO non è null
            if (utenteDto.Username != null)
            {
                utenteToUpdate.Username = utenteDto.Username;
            }
            if (utenteDto.Email != null)
            {
                utenteToUpdate.Email = utenteDto.Email;
            }

            // IsActive è un booleano, quindi è sicuro aggiornarlo direttamente
            utenteToUpdate.IsActive = utenteDto.IsActive;

            if (utenteDto.Password != null)
            {
                utenteToUpdate.PasswordHash = HashPassword(utenteDto.Password);
            }

            if (utenteDto.ImageUrl != null)
            {
                utenteToUpdate.ImageUrl = utenteDto.ImageUrl;
            }

            try
            {
                await _context.SaveChangesAsync();
                return Ok(utenteToUpdate);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UtentiExists(id))
                {
                    return NotFound($"Utente con ID {id} non trovato.");
                }
                else
                {
                    throw;
                }
            }
        }

        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }


        // DELETE: api/Utenti/5
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin,Moderatore,Utente")]
        public async Task<IActionResult> SoftDeleteUtente(int id)
        {
            var utente = await _context.Utentis.FindAsync(id);
            if (utente == null)
            {
                return NotFound($"Utente con ID {id} non trovato.");
            }

            utente.IsActive = false;
            try
            {
                await _context.SaveChangesAsync();
                return Ok($"Utente con ID {id} disattivato con successo.");
            }
            catch (Exception ex)
            {
                // Log dell'eccezione con il tuo sistema di logging qui
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Si è verificato un errore durante l'operazione di disattivazione.");
            }
        }

        // PATCH: api/Utenti/5/restore
        [HttpPatch("{id}/restore")]
        [Authorize(Roles = "Admin,Moderatore")]
        public async Task<IActionResult> RestoreUtente(int id)
        {
            var utente = await _context.Utentis.FindAsync(id);
            if (utente == null)
            {
                return NotFound($"Utente con ID {id} non trovato.");
            }

            if (utente.IsActive)
            {
                return BadRequest($"Utente con ID {id} è già attivo.");
            }

            utente.IsActive = true;
            try
            {
                await _context.SaveChangesAsync();
                return Ok($"Utente con ID {id} è stato riattivato con successo.");
            }
            catch (Exception ex)
            {

                return StatusCode(StatusCodes.Status500InternalServerError,
                    "Si è verificato un errore durante il tentativo di riattivare l'utente.");
            }
        }

        [HttpPost("upload-image/{id}")]
        [Authorize(Roles = "Admin,Moderatore,Utente")]
        public async Task<IActionResult> UploadProfileImage(int id, [FromForm] IFormFile file)
        {
            // Verifica se l'utente esiste
            var utente = await _context.Utentis.FindAsync(id);
            if (utente == null)
            {
                return NotFound("Utente non trovato.");
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest("File non valido.");
            }

            // Crea un nome file univoco utilizzando Guid e l'estensione originale del file
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var relativePath = Path.Combine("uploads", "profile", fileName);
            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), _profileImagesPath, fileName);

            // Assicurati che la cartella di destinazione esista
            var directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Aggiorna il percorso dell'immagine nel database
            utente.ImageUrl = relativePath; // Assicurati che questo percorso sia accessibile tramite il tuo server web

            _context.Utentis.Update(utente);
            await _context.SaveChangesAsync();

            return Ok(new { imagePath = utente.ImageUrl });
        }



        private bool UtentiExists(int id)
        {
            return _context.Utentis.Any(e => e.UserId == id);
        }
    }

}
