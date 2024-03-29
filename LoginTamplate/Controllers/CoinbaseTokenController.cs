//using LoginTamplate.Model;
//using Microsoft.AspNetCore.Mvc;

//namespace LoginTemplate.Controllers
//{
//    [ApiController]
//    [Route("[controller]")]
//    public class CoinbaseTokenController : ControllerBase
//    {
//        private readonly CoinbaseAuthentication _coinbaseAuth;

//        public CoinbaseTokenController(IConfiguration configuration)
//        {
//            var apiKey = configuration["Coinbase:ApiKey"];
//            var apiSecret = configuration["Coinbase:ApiSecret"];

//            _coinbaseAuth = new CoinbaseAuthentication(apiKey, apiSecret);
//        }

//        [HttpGet("my-accounts")]
//        public async Task<IActionResult> GetCoinbaseTokenAsync()
//        {
//            var timestamp = _coinbaseAuth.GenerateTimeStamp();
//            var method = HttpMethod.Get;

//            // Assicurati di usare il percorso completo come specificato nella documentazione API di Coinbase.
//            var requestPath = "/api/v3/brokerage/accounts"; // Aggiorna con il path corretto come da documentazione

//            var body = ""; // Il body va incluso solo se la richiesta è di tipo POST

//            var signature = _coinbaseAuth.GenerateSignature(timestamp, method.Method, requestPath, body);

//            using var client = new HttpClient();

//            // Costruisci l'URL completo per la richiesta
//            var requestUrl = "https://api.coinbase.com" + requestPath;
//            var request = new HttpRequestMessage(method, requestUrl);

//            // Aggiungere gli header necessari per l'autenticazione
//            _coinbaseAuth.AddHeaders(request, timestamp, signature);

//            var response = await client.SendAsync(request);

//            if (!response.IsSuccessStatusCode)
//            {
//                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
//            }

//            var content = await response.Content.ReadAsStringAsync();
//            return Ok(content);
//        }

//        [HttpGet("all-products")]
//        public async Task<IActionResult> GetAllProductsAsync()
//        {
//            using var client = new HttpClient();
//            var requestUrl = "https://api.exchange.coinbase.com/products"; // URL per API pubbliche

//            // Aggiungi un User-Agent alla richiesta
//            client.DefaultRequestHeaders.Add("User-Agent", "CoinbaseClient/1.0");

//            var response = await client.GetAsync(requestUrl);

//            if (!response.IsSuccessStatusCode)
//            {
//                return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
//            }

//            var content = await response.Content.ReadAsStringAsync();
//            return Ok(content);
//        }


//        private class CoinbaseKeys
//        {
//            public string Name { get; set; }
//            public string Principal { get; set; }
//            public string PrincipalType { get; set; }
//            public string PublicKey { get; set; }
//            public string PrivateKey { get; set; }
//            public DateTime CreateTime { get; set; }
//            public string ProjectId { get; set; }
//            public string Nickname { get; set; }
//            public List<string> Scopes { get; set; }
//            public List<string> AllowedIps { get; set; }
//            public string KeyType { get; set; }
//            public bool Enabled { get; set; }
//            public List<string> LegacyScopes { get; set; }
//            public string CreatedByUserId { get; set; }
//            public string CreatedByUserMongoId { get; set; }
//        }
//    }
//}
