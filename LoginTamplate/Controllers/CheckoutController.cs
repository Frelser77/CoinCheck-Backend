using LoginTamplate.Model.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
namespace LoginTamplate.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CheckoutController : ControllerBase
    {
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
    }
}
