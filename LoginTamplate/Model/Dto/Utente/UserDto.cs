﻿namespace LoginTamplate.Model.Dto.Utente
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Ruolo { get; set; }
        public string? ImageUrl { get; set; }
    }

}
