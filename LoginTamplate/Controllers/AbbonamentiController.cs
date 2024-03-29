using LoginTamplate.Model;
using LoginTamplate.Model.Dto.Abbonamento;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoginTamplate.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AbbonamentiController : ControllerBase
    {
        private readonly CoinCheckContext _context;

        public AbbonamentiController(CoinCheckContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAbbonamenti()
        {
            var abbonamenti = _context.Abbonamentis.ToList();
            return Ok(abbonamenti);
        }
        // Aggiungi un nuovo abbonamento
        [HttpPost]
        public async Task<IActionResult> AddAbbonamento([FromBody] Abbonamenti abbonamento)
        {
            _context.Abbonamentis.Add(abbonamento);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAbbonamenti), new { id = abbonamento.Idprodotto }, abbonamento);
        }

        // Modifica un abbonamento esistente
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAbbonamento(int id, [FromBody] AbbonamentoUpdateDTO abbonamentoDTO)
        {
            var abbonamento = await _context.Abbonamentis.FindAsync(id);
            if (abbonamento == null)
            {
                return NotFound();
            }

            // Aggiorna solo i campi che sono stati effettivamente modificati
            if (abbonamentoDTO.Prezzo.HasValue)
                abbonamento.Prezzo = abbonamentoDTO.Prezzo.Value;

            if (!string.IsNullOrWhiteSpace(abbonamentoDTO.TipoAbbonamento))
                abbonamento.TipoAbbonamento = abbonamentoDTO.TipoAbbonamento;

            if (!string.IsNullOrWhiteSpace(abbonamentoDTO.Descrizione))
                abbonamento.Descrizione = abbonamentoDTO.Descrizione;

            if (abbonamentoDTO.Quantita.HasValue)
                abbonamento.Quantita = abbonamentoDTO.Quantita.Value;

            if (!string.IsNullOrWhiteSpace(abbonamentoDTO.ImageUrl))
                abbonamento.ImageUrl = abbonamentoDTO.ImageUrl;

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

        // Elimina un abbonamento
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