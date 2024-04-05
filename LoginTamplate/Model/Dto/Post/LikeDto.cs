namespace LoginTamplate.Model.Dto.Post
{
    public class LikeDto
    {
        public int LikeId { get; set; }
        public int? PostId { get; set; }
        public int? CommentId { get; set; }

        public int UserId { get; set; }
        public UserDto User { get; set; }
    }
}
