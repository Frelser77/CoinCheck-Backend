using LoginTamplate.Data;
using LoginTamplate.Model;
using LoginTamplate.Model.Dto.Email;
using LoginTamplate.Model.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly IEmailService _emailService;

        public CheckoutController(CoinCheckContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _stripeWebhookSecret = _configuration["Stripe:WebhookSecret"];
            _emailService = emailService;
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
                            Images = new List<string> { item.ImageUrl.Replace("uploads/products/", "").Replace("\\", "/") },
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

                    var prodotto = await _context.Abbonamentis
                                     .Where(p => p.Idprodotto == idProdotto)
                                     .SingleOrDefaultAsync();

                    // Assicurati di avere i dettagli dell'utente e del prodotto
                    if (prodotto != null && utente != null)
                    {
                        // Prepara il corpo dell'email
                        string imageUrl = prodotto.ImageUrl.Replace("uploads/products/", "").Replace("\\", "/");
                        string body = $@"
                                    <html>
                                    <head>
                                        <title>Acquisto CoinCheck</title>
                                    </head>
                                    <body>
                                        <p>Ciao {utente.Username},</p>
                                        <p>Grazie per il tuo acquisto di: <strong>{prodotto.TipoAbbonamento}</strong>.</p>
                                        <p>Descrizione: {prodotto.Descrizione}.</p>
                                        <img src='https://storage.googleapis.com/immagine-abbonamenti/{imageUrl}' alt='{prodotto.TipoAbbonamento}' style='max-width:600px;'/><br/>
                                        <p>La tua sottoscrizione scadrà il: <strong>{dataScadenza.ToString("dd/MM/yyyy")}</strong>.</p>
                                        <p>Grazie per aver scelto CoinCheck.</p>
                                    </body>
                                    </html>";


                        // Crea l'email da inviare
                        EmailDto emailDto = new EmailDto
                        {
                            to = utente.Email,
                            subject = "Dettagli dell'acquisto CoinCheck",
                            body = body
                        };

                        // Invia l'email
                        await _emailService.SendEmailAsync(emailDto);
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
