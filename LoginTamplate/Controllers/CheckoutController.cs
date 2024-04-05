using LoginTamplate.Model;
using LoginTamplate.Model.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace LoginTamplate.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class CheckoutController : ControllerBase
    {
        private readonly CoinCheckContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _stripeWebhookSecret;

        public CheckoutController(CoinCheckContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _stripeWebhookSecret = _configuration["Stripe:WebhookSecret"];
        }

        [HttpPost("create-session")]
        public ActionResult CreateCheckoutSession([FromBody] CreateCheckoutSessionRequest request)
        {
            var domain = "http://localhost:5173"; // URL del tuo frontend React

            // Crea la lista degli articoli per la sessione di checkout
            var lineItems = request
                .Items.Select(item => new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "eur",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.TipoAbbonamento,
                            Description = item.Descrizione,
                            //Images = new List<string> { item.ImageUrl },
                        },
                        UnitAmountDecimal = (long)(item.Prezzo * 100), // Converti il prezzo in centesimi
                    },
                    Quantity = 1, // Modifica la quantità se necessario    item.Quantita
                })
                .ToList();

            // Crea opzioni per la sessione di checkout
            var options = new SessionCreateOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    { "UserID", request.UserId.ToString() },
                    { "IDProdotto", request.IdProdotto.ToString() }
                },

                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = domain + "/",
                CancelUrl = domain + "/abbonamenti",
            };

            var service = new SessionService();
            Session session = service.Create(options);

            return Ok(new { sessionId = session.Id });
        }

        [AllowAnonymous]
        [HttpPost("webhook")]
        public async Task<IActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _stripeWebhookSecret
                );

                if (stripeEvent.Type == Events.CheckoutSessionCompleted)
                {
                    var session = stripeEvent.Data.Object as Session;

                    // Recupera i metadati
                    int userId = int.Parse(session.Metadata["UserID"]);
                    int idProdotto = int.Parse(session.Metadata["IDProdotto"]);

                    // Calcola DataScadenza basandoti sul tipo di abbonamento
                    DateTime dataScadenza;
                    switch (idProdotto)
                    {
                        case 1: // ID dell'abbonamento Base
                            dataScadenza = DateTime.UtcNow.AddMonths(1);
                            break;
                        case 2: // ID dell'abbonamento Medium
                            dataScadenza = DateTime.UtcNow.AddMonths(3);
                            break;
                        case 3: // ID dell'abbonamento Pro
                            dataScadenza = DateTime.UtcNow.AddYears(1);
                            break;
                        default:
                            throw new Exception("IDProdotto non valido.");
                    }

                    // Aggiungi l'acquisto in AcquistiAbbonamenti
                    var acquisto = new AcquistiAbbonamenti
                    {
                        UserId = userId,
                        Idprodotto = idProdotto,
                        DataAcquisto = DateTime.UtcNow,
                        DataScadenza = dataScadenza
                    };
                    _context.AcquistiAbbonamentis.Add(acquisto);

                    // Aggiorna il ruolo dell'utente
                    var utente = await _context.Utentis.FindAsync(userId);
                    if (utente != null)
                    {
                        switch (idProdotto)
                        {
                            case 1:
                                utente.RuoloId = 4;
                                break;
                            case 2:
                                utente.RuoloId = 5;
                                break;
                            case 3:
                                utente.RuoloId = 6;
                                break;
                        }
                        _context.Utentis.Update(utente);
                    }

                    await _context.SaveChangesAsync();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                // Considera di loggare l'eccezione
                return BadRequest(new { message = ex.Message });
            }
        }



        private int GetUserIdFromClaims(HttpContext context)
        {
            if (context.User.Identity is ClaimsIdentity identity)
            {
                var userClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
                if (userClaim != null)
                {
                    return int.Parse(userClaim.Value);
                }
            }
            throw new Exception("User Claim not found");
        }
    }
}
