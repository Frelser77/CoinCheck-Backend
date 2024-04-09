namespace LoginTamplate.Model.Dto.Post
{
    public class CrudPostDto
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public IFormFile? File { get; set; }

    }
}
