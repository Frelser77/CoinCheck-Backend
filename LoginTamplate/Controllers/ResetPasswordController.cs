using LoginTamplate.Data;
using LoginTamplate.Model;
using LoginTamplate.Model.Dto.Email;
using LoginTamplate.Model.Dto.PasswordReset;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LoginTamplate.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ResetPasswordController : ControllerBase
    {
        private readonly CoinCheckContext _context;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public ResetPasswordController(CoinCheckContext context, IConfiguration configuration, IEmailService emailService)
        {

            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        [HttpPost("passwordResetRequest")]
        public async Task<IActionResult> PasswordResetRequest([FromBody] PasswordResetRequestDto request)
        {
            var user = await _context.Utentis.SingleOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                return Ok(new { Success = true, Message = "Se l'indirizzo email corrisponde a un account, verrà inviato un link per il reset della password." });
            }

            var token = GeneratePasswordResetToken(user);

            // Salva il token nel database
            var passwordResetToken = new PasswordResetToken
            {
                UserId = user.UserId,
                Token = token,
                ExpiryDate = DateTime.UtcNow.AddHours(3)
            };

            _context.PasswordResetTokens.Add(passwordResetToken);
            await _context.SaveChangesAsync();

            var callbackUrl = $"http://localhost:5173/reset-password/{token}/{user.UserId}";
            await _emailService.SendEmailAsync(new EmailDto
            {
                to = user.Email,
                subject = "Reset Password",
                body = $"Per resettare la tua password, clicca <a href='{callbackUrl}'>qui</a>"
            });

            return Ok(new { Success = true, Message = "Email per il reset della password inviata con successo." });
        }


        private string GeneratePasswordResetToken(Utenti user)
        {
            var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var key = System.Text.Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("nameid", user.UserId.ToString()),
                    new Claim("reset", "true"),
                }),
                Expires = DateTime.UtcNow.AddHours(3),

                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }


        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

        [HttpPost("resetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {

            var user = await _context.Utentis.FindAsync(resetPasswordDto.UserId);
            if (user == null)
            {
                return BadRequest(new { Success = false, Message = "Token non valido o scaduto o utente non trovato." });
            }


            var tokenRecord = await _context.PasswordResetTokens
             .Where(t => t.UserId == resetPasswordDto.UserId && t.ExpiryDate > DateTime.UtcNow)
             .OrderByDescending(t => t.ExpiryDate)
             .FirstOrDefaultAsync(t => t.Token == resetPasswordDto.Token);
            if (tokenRecord == null)
            {
                return BadRequest("Token non valido o scaduto.");
            }


            if (tokenRecord.ExpiryDate <= DateTime.UtcNow)
            {
                return BadRequest("Token non valido o scaduto.");
            }

            if (!IsValidResetToken(resetPasswordDto.Token, user.UserId))
            {
                return BadRequest("Token non valido o scaduto.");
            }

            user.PasswordHash = HashPassword(resetPasswordDto.NewPassword);

            await _context.SaveChangesAsync();
            _context.PasswordResetTokens.Remove(tokenRecord);
            await _context.SaveChangesAsync();

            // Invia una notifica via email all'utente che la password è stata cambiata.
            var changeDateTime = DateTime.UtcNow;

            // Formatta la data e l'ora in una stringa leggibile
            var formattedDateTime = changeDateTime.ToString("f"); // 'f' è un formato che include la data completa e l'ora nel formato lungo.

            var notificationEmail = new EmailDto
            {
                to = user.Email,
                subject = "Notifica di Cambio Password",
                body = $"La password per il tuo account è stata cambiata con successo il {formattedDateTime}. " +
                       "Se non hai richiesto tu questa modifica, ti preghiamo di contattare immediatamente l'assistenza."
            };


            await _emailService.SendEmailAsync(notificationEmail);

            return Ok(new { Success = true, Message = "Password resettata con successo." });
        }



        private bool IsValidResetToken(string token, int userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = System.Text.Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    ClockSkew = TimeSpan.FromMinutes(5)
                };

                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;

                var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "nameid");
                if (userIdClaim == null)
                {
                    return false;
                }

                // Log aggiuntivo per verificare il valore del claim

                if (!int.TryParse(userIdClaim.Value, out int tokenUserId))
                {
                    return false;
                }

                bool isResetToken = jwtToken.Claims.Any(x => x.Type == "reset" && x.Value == "true");
                if (!isResetToken)
                {
                    return false;
                }

                bool userIdMatches = tokenUserId == userId;
                if (!userIdMatches)
                {
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }


    }
}
