namespace LoginTamplate.Model.Dto.Post
{
    public class PostDto
    {
        public PostDto()
        {
            Comments = new List<CommentDto>();
            Likes = new List<LikeDto>();
        }

        public int PostId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserImage { get; set; }
        public string UserRole { get; set; }
        public string? Title { get; set; }
        public string Content { get; set; }
        public string FilePath { get; set; }
        public DateTime PostDate { get; set; }
        public int LikeCount { get; set; }
        public List<CommentDto> Comments { get; set; }
        public List<LikeDto> Likes { get; set; }
    }
}
