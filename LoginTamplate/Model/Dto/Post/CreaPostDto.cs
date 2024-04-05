namespace LoginTamplate.Model.Dto.Post
{
    public class CreatePostDto
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public IFormFile? File { get; set; }

    }
}
