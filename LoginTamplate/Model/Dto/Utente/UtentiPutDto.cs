namespace LoginTamplate.Model.Dto.Utente
{
    public class UtentiPutDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }

        public bool IsActive { get; set; }

        public string? ImageUrl { get; set; }

    }
}
