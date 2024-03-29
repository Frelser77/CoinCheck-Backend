using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LoginTemplate.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        // Metodo che richiede l'autenticazione e verifica se l'utente ha il ruolo di amministratore
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {
            // Se l'utente è autenticato e ha il ruolo di admin, ritorna Ok, altrimenti non verrà neanche qui.
            return Ok(new { message = "Accesso consentito: l'utente è un amministratore." });
        }
    }
}