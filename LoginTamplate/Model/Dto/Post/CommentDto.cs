namespace LoginTamplate.Model.Dto.Post
{
    public class CommentDto
    {
        public int CommentId { get; set; }
        public int PostId { get; set; }
        public string Content { get; set; }
        public DateTime CommentDate { get; set; }
        public int UserId { get; set; }
        public int LikeCount { get; set; }
        public List<LikeDto> Likes { get; set; }
        public string UserName { get; set; } // Aggiungi il campo per l'username dell'utente
        public string UserImage { get; set; } // Aggiungi il campo per l'immagine dell'utente
    }
}
