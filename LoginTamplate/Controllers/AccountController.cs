using LoginTamplate.Data;
using LoginTamplate.Model;
using LoginTamplate.Model.Dto.Auth;
using LoginTamplate.Model.Dto.Email;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace LoginTamplate.Controllers
{
    [Route("[controller]")]
    [ApiController] // Usa ApiController per le API
    public class AccountController : ControllerBase // Usa ControllerBase per le API
    {
        private readonly CoinCheckContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        // Utilizza l'iniezione delle dipendenze per passare il contesto e la configurazione
        public AccountController(CoinCheckContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await Authenticate(loginDto);

            if (user != null)
            {
                var tokenString = BuildToken(user);

                LogAttivitum logAttivita = new LogAttivitum
                {
                    UserId = user.UserId,
                    Timestamp = DateTime.UtcNow,
                    Azione = "Login"
                };

                _context.LogAttivita.Add(logAttivita);
                await _context.SaveChangesAsync();

                // Crea un oggetto anonimo o una DTO specifica per passare le informazioni dell'utente desiderate al front-end
                var userDto = new
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    ImageUrl = user.ImageUrl,
                    logAttivita.Timestamp
                };

                return Ok(new { token = tokenString, user = userDto });
            }

            return Unauthorized();
        }


        private string BuildToken(Utenti user)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(JwtRegisteredClaimNames.Sub, user.Username),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(ClaimTypes.Role, user.Ruolo.NomeRuolo)
        // Altre eventuali claims
    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
             _configuration["Jwt:Issuer"],
             _configuration["Jwt:Audience"],
             claims,
             expires: DateTime.UtcNow.AddMinutes(30), // Imposta la scadenza a 30 minuti
             signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Metodo per autenticare l'utente e restituire l'oggetto Utenti se l'autenticazione ha successo o null se fallisce
        private async Task<Utenti> Authenticate(LoginDto loginDto)
        {
            var user = await _context.Utentis
                                     .Include(u => u.Ruolo)
                                     .SingleOrDefaultAsync(u => u.Username == loginDto.Username);

            if (user != null && VerifyPasswordHash(loginDto.Password, user.PasswordHash))
            {
                return user;
            }

            return null;
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            // Verifica se l'username esiste già
            if (await _context.Utentis.AnyAsync(u => u.Username == registerDto.Username))
            {
                return BadRequest("Username is already taken.");
            }

            string defaultProfileImageUrl = "uploads/profile/placeholder-profile.png\"";

            // Crea un nuovo utente con la password hashata usando BCrypt
            Utenti user = new Utenti
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = HashPassword(registerDto.Password),
                IsActive = true,
                RuoloId = 1,
                ImageUrl = defaultProfileImageUrl
            };

            _context.Utentis.Add(user);
            await _context.SaveChangesAsync();

            string welcomeMessage = $"Ciao {registerDto.Username},\n\n" +
                            "Grazie per esserti registrato/a su CoinCheck! " +
                            "Ora puoi accedere e iniziare ad usare i nostri servizi.\n\n" +
                            "Cordiali saluti,\nIl team di CoinCheck";

            EmailDto emailDto = new EmailDto
            {
                to = registerDto.Email,
                subject = "Benvenuto/a su CoinCheck!",
                body = welcomeMessage
            };

            // Invia l'email in modo asincrono
            await _emailService.SendEmailAsync(emailDto);

            // Ora puoi ritornare solo un messaggio di successo senza token
            return Ok(new { message = "User registered successfully" });
        }
    }
}
