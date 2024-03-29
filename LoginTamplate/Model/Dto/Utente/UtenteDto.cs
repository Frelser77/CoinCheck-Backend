namespace LoginTamplate.Model.Dto.Utente
{
    public class UtenteDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public string? ImageUrl { get; set; }
        public IEnumerable<string> Comments { get; set; }
        public IEnumerable<string> LogAttivita { get; set; }
        public IEnumerable<string> Posts { get; set; }
        public List<PreferenzeUtenteDto> PreferenzeUtentes { get; set; }
    }
}
