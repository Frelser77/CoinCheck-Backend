//using System.Security.Cryptography;
//using System.Text;

//namespace LoginTamplate.Model
//{
//    public class CoinbaseAuthentication
//    {
//        private string apiKey;
//        private string apiSecret;
//        //private readonly string passphrase;

//        public CoinbaseAuthentication(string apiKey, string apiSecret)
//        {
//            this.apiKey = apiKey;
//            this.apiSecret = apiSecret;
//        }

//        public string GenerateTimeStamp()
//        {
//            return ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString();
//        }

//        public string GenerateSignature(string timestamp, string method, string requestPath, string body = "")
//        {
//            var privateKeyCleaned = apiSecret
//                .Replace("-----BEGIN EC PRIVATE KEY-----", "")
//                .Replace("-----END EC PRIVATE KEY-----", "")
//                .Replace("\n", "")
//                .Replace("\r", "")
//                .Trim();

//            byte[] privateKeyBytes;
//            try
//            {
//                privateKeyBytes = Convert.FromBase64String(privateKeyCleaned); // Prova a convertire la chiave pulita in un array di byte.
//            }
//            catch (FormatException fe)
//            {
//                // Gestisci l'errore se la stringa non è una stringa Base64 valida.
//                throw new ArgumentException("La chiave privata non è in un formato Base64 valido.", fe);
//            }

//            var prehash = timestamp + method.ToUpper() + requestPath + (body ?? "");
//            using var hmac = new HMACSHA256(privateKeyBytes);
//            byte[] bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(prehash));
//            return Convert.ToBase64String(bytes);
//        }

//        public void AddHeaders(HttpRequestMessage request, string timestamp, string signature)
//        {
//            // Log dei valori per il debug
//            Console.WriteLine("Aggiunta degli header alla richiesta:");
//            Console.WriteLine($"CB-ACCESS-SIGN: {signature}");
//            Console.WriteLine($"CB-ACCESS-TIMESTAMP: {timestamp}");
//            Console.WriteLine($"CB-ACCESS-KEY: {apiKey}");

//            signature = CleanHeaderValue(signature);
//            timestamp = CleanHeaderValue(timestamp);
//            apiKey = CleanHeaderValue(apiKey); // Se apiKey è immutabile, usa una variabile locale

//            request.Headers.Add("CB-ACCESS-SIGN", signature);
//            request.Headers.Add("CB-ACCESS-TIMESTAMP", timestamp);
//            request.Headers.Add("CB-ACCESS-KEY", apiKey);
//            // Aggiungi altri header necessari qui
//        }
//        private string CleanHeaderValue(string value)
//        {
//            // Rimuovi tutti i caratteri di ritorno a capo e gli spazi bianchi extra
//            return value.Replace("\n", "").Replace("\r", "").Trim();
//        }

//    }
//}
