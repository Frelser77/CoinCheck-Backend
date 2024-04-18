using Google.Cloud.Storage.V1;
using LoginTamplate.Model;
using LoginTamplate.Model.Dto.Abbonamento;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoginTamplate.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AbbonamentiController : ControllerBase
    {
        private readonly CoinCheckContext _context;
        private readonly StorageClient _storageClient;
        private const string BucketName = "immagine-abbonamenti";
        private readonly UrlSigner _urlSigner;
        private readonly ILogger<AbbonamentiController> _logger;

        public AbbonamentiController(CoinCheckContext context, StorageClient storageClient, UrlSigner urlSigner, ILogger<AbbonamentiController> logger)
        {
            _context = context;
            _storageClient = storageClient;
            _urlSigner = urlSigner;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetAbbonamenti()
        {
            var abbonamenti = _context.Abbonamentis.ToList();
            return Ok(abbonamenti);
        }

        // Aggiungi un nuovo abbonamento
        [Authorize(Roles = "Admin,Moderatore")]
        [HttpPost]
        public async Task<IActionResult> AddAbbonamento([FromBody] Abbonamenti abbonamento)
        {
            _context.Abbonamentis.Add(abbonamento);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAbbonamenti), new { id = abbonamento.Idprodotto }, abbonamento);
        }

        // Aggiorna un abbonamento esistente 
        [Authorize(Roles = "Admin,Moderatore")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAbbonamento(int id, [FromForm] AbbonamentoUpdateDTO abbonamentoDTO)
        {
            var abbonamento = await _context.Abbonamentis.FindAsync(id);
            if (abbonamento == null)
            {
                return NotFound();
            }

            // Aggiorna i campi testuali
            abbonamento.Prezzo = abbonamentoDTO.Prezzo ?? abbonamento.Prezzo;
            abbonamento.TipoAbbonamento = abbonamentoDTO.TipoAbbonamento ?? abbonamento.TipoAbbonamento;
            abbonamento.Descrizione = abbonamentoDTO.Descrizione ?? abbonamento.Descrizione;
            abbonamento.Quantita = abbonamentoDTO.Quantita ?? abbonamento.Quantita;

            _logger.LogInformation($"Prezzo ricevuto dal form: {abbonamentoDTO.Prezzo}");

            // Gestione dell'upload dell'immagine
            var file = Request.Form.Files.FirstOrDefault();
            if (file != null && file.Length > 0)
            {
                // Controlla il formato del file
                var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp" };
                if (!allowedMimeTypes.Contains(file.ContentType))
                {
                    return BadRequest("Formato del file non consentito.");
                }

                try
                {
                    var storage = StorageClient.Create();
                    var bucketName = BucketName;
                    var objectName = Guid.NewGuid().ToString();
                    var imageObject = new Google.Apis.Storage.v1.Data.Object()
                    {
                        Bucket = bucketName,
                        Name = objectName,
                        ContentType = file.ContentType
                    };

                    using (var stream = file.OpenReadStream())
                    {
                        await storage.UploadObjectAsync(imageObject, stream);
                    }
                    abbonamento.ImageUrl = $"https://storage.googleapis.com/{bucketName}/{objectName}";
                }
                catch (Exception ex)
                {
                    // Qui puoi gestire le eccezioni specifiche legate all'upload su GCS
                    return StatusCode(500, $"Errore durante l'upload dell'immagine: {ex.Message}");
                }
            }

            _context.Entry(abbonamento).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Abbonamentis.Any(e => e.Idprodotto == id))
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

        [Authorize(Roles = "Admin,Moderatore")]
        [HttpGet("generate-signed-url")]
        public async Task<IActionResult> GenerateSignedUrl(string fileName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileName))
                {
                    return BadRequest("Il nome del file non può essere vuoto.");
                }

                string objectName = Guid.NewGuid().ToString() + Path.GetExtension(fileName);

                var url = await _urlSigner.SignAsync(
                    BucketName, // bucketName
                    objectName, // objectName
                    TimeSpan.FromMinutes(10), // expiration
                    HttpMethod.Put); // requestMethod

                return Ok(new { SignedUrl = url });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Errore durante la generazione dell'URL firmato: {ex.Message}");
            }
        }


        // Elimina un abbonamento
        [Authorize(Roles = "Admin,Moderatore")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAbbonamento(int id)
        {
            var abbonamento = await _context.Abbonamentis.FindAsync(id);
            if (abbonamento == null)
            {
                return NotFound();
            }

            _context.Abbonamentis.Remove(abbonamento);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}