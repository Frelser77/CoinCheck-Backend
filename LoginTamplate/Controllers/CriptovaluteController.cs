using LoginTamplate.Model;
using LoginTamplate.Model.Dto.Criptovaluta;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoginTamplate.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CriptovaluteController : ControllerBase
    {
        private readonly CoinCheckContext _context;

        public CriptovaluteController(CoinCheckContext context)
        {
            _context = context;
        }

        [HttpPost("addCoins")]
        public async Task<ActionResult<Criptovalute>> PostCriptovaluta(CriptovalutaDto criptoDto)
        {
            // Cerca una criptovaluta esistente per nome o simbolo
            var existingCriptovaluta = _context.Criptovalutes
                .FirstOrDefault(c => c.Nome == criptoDto.Nome || c.Simbolo == criptoDto.Simbolo);

            if (existingCriptovaluta != null)
            {
                // Aggiorna i dati della criptovaluta esistente
                existingCriptovaluta.PrezzoUsd = criptoDto.PrezzoUsd;
                existingCriptovaluta.Variazione24h = criptoDto.Variazione24h;
                existingCriptovaluta.Volume24h = criptoDto.Volume24h;
                existingCriptovaluta.UltimoAggiornamento = DateTime.UtcNow; // Imposta la data e l'ora attuale in UTC

                _context.Entry(existingCriptovaluta).State = EntityState.Modified;
            }
            else
            {
                // Se non esiste, crea una nuova criptovaluta
                var criptovaluta = new Criptovalute
                {
                    Nome = criptoDto.Nome,
                    Simbolo = criptoDto.Simbolo,
                    PrezzoUsd = criptoDto.PrezzoUsd,
                    Variazione24h = criptoDto.Variazione24h,
                    Volume24h = criptoDto.Volume24h,
                    UltimoAggiornamento = DateTime.UtcNow, // Imposta la data e l'ora attuale in UTC
                    IsActive = true
                };

                _context.Criptovalutes.Add(criptovaluta);
            }

            await _context.SaveChangesAsync();

            // Restituisci la criptovaluta aggiornata o creata
            return Ok(existingCriptovaluta ?? _context.Criptovalutes
                .FirstOrDefault(c => c.Nome == criptoDto.Nome || c.Simbolo == criptoDto.Simbolo));
        }

        [HttpPost("togglePreferenza")]
        public async Task<IActionResult> TogglePreferenza([FromBody] TogglePreferenzaDto preferenzaDto)
        {
            var utente = await _context.Utentis.FindAsync(preferenzaDto.UserId);
            var cripto = await _context.Criptovalutes.FindAsync(preferenzaDto.CriptoId);

            if (utente == null || cripto == null)
            {
                return NotFound("Utente o Criptovaluta non trovati");
            }

            var preferenzaEsistente = await _context.PreferenzeUtentes
                .FirstOrDefaultAsync(p => p.UserId == preferenzaDto.UserId && p.CriptoId == preferenzaDto.CriptoId);

            if (preferenzaEsistente != null)
            {
                // Se la preferenza esiste, la rimuoviamo
                _context.PreferenzeUtentes.Remove(preferenzaEsistente);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Preferenza rimossa" });
            }
            else
            {
                // Se la preferenza non esiste, la aggiungiamo
                var nuovaPreferenza = new PreferenzeUtente
                {
                    UserId = utente.UserId,
                    CriptoId = cripto.CriptoId,
                    // Imposta i valori di default o basati su input
                    NotificaSogliaPrezzo = false,
                    SogliaPrezzo = null
                };

                _context.PreferenzeUtentes.Add(nuovaPreferenza);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Preferenza aggiunta" });
            }
        }

        [HttpGet("preferenzeUtente/{userId}")]
        public async Task<ActionResult<IEnumerable<PreferenzaDto>>> GetPreferenzeUtente(int userId)
        {
            var preferenzeUtente = await _context.PreferenzeUtentes
                .Where(p => p.UserId == userId)
                .Include(p => p.Cripto)
                .Select(p => new PreferenzaDto
                {
                    PreferenzaId = p.PreferenzaId,
                    UserId = p.UserId,
                    CriptoId = p.Cripto.CriptoId,
                    CriptoNome = p.Cripto.Nome
                })
                .ToListAsync();

            if (!preferenzeUtente.Any())
            {
                return NotFound("Nessuna preferenza trovata per l'utente.");
            }

            return Ok(preferenzeUtente);
        }

        [HttpGet("GetCriptoIdByName/{criptoName}")]
        public async Task<ActionResult<int>> GetCriptoIdByName(string criptoName)
        {
            var cripto = await _context.Criptovalutes
                              .FirstOrDefaultAsync(c => c.Nome == criptoName);

            if (cripto == null)
            {
                return NotFound("Criptovaluta non trovata");
            }

            return Ok(cripto.CriptoId);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Criptovalute>> GetCriptovaluta(int id)
        {
            var criptovaluta = await _context.Criptovalutes.FindAsync(id);

            if (criptovaluta == null)
            {
                return NotFound();
            }

            return criptovaluta;
        }
    }

}
