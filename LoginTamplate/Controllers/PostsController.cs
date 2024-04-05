using LoginTamplate.Model;
using LoginTamplate.Model.Dto.Comment;
using LoginTamplate.Model.Dto.Post;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LoginTamplate.Controllers
{


    [ApiController]
    [Route("[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly CoinCheckContext _context;

        public PostsController(CoinCheckContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("all")]
        public IActionResult GetAllPosts()
        {
            var postEntities = _context.Posts
         .Include(p => p.LikesNavigation).ThenInclude(l => l.User)
         .Include(p => p.User).ThenInclude(u => u.Ruolo)
         .Include(p => p.Comments).ThenInclude(c => c.User)
         .Include(p => p.Comments).ThenInclude(c => c.Likes).ThenInclude(l => l.User)
         .Where(p => p.IsActive)
         .ToList();


            var posts = postEntities.Select(p => new PostDto
            {
                PostId = p.PostId,
                UserId = p.UserId,
                UserImage = p.User.ImageUrl,
                UserName = p.User.Username,
                UserRole = p.User.Ruolo?.NomeRuolo,
                Title = p.Title,
                Content = p.Content,
                FilePath = p.FilePath,
                PostDate = p.PostDate,
                LikeCount = p.LikesNavigation?.Count() ?? 0,
                Likes = p.LikesNavigation != null ? p.LikesNavigation.Select(l => new LikeDto
                {
                    LikeId = l.LikeId,
                    UserId = l.UserId,
                    User = GetUserDto(l.UserId)
                }).ToList() : new List<LikeDto>(),

                Comments = p.Comments != null ? p.Comments
                .Where(c => c.IsActive)
                .Select(c => new CommentDto
                {
                    CommentId = c.CommentId,
                    PostId = c.PostId,
                    Content = c.Content,
                    CommentDate = c.CommentDate,
                    UserId = c.UserId,
                    LikeCount = c.Likes != null ? c.Likes.Count() : 0,
                    Likes = c.Likes != null ? c.Likes.Select(l => new LikeDto
                    {
                        LikeId = l.LikeId,
                        UserId = l.UserId,
                        User = GetUserDto(l.UserId)
                    }).ToList() : new List<LikeDto>(),
                    UserName = c.User.Username,
                    UserImage = c.User.ImageUrl
                }).ToList() : new List<CommentDto>(),
            }).ToList();

            return Ok(posts);
        }

        private UserDto GetUserDto(int userId)
        {
            var user = _context.Utentis
                .Include(u => u.Ruolo).SingleOrDefault(u => u.UserId == userId);

            if (user == null) return null;

            return new UserDto
            {
                UserId = user.UserId,
                Username = user.Username,
                ImageUrl = user.ImageUrl,
                RoleName = user.Ruolo?.NomeRuolo
            };
        }

        [HttpPost("post"), DisableRequestSizeLimit]
        public async Task<IActionResult> CreatePost([FromForm] CreatePostDto createPostDto)
        {
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"Claim Type: {claim.Type}, Claim Value: {claim.Value}");
            }

            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            {
                return BadRequest("Invalid user ID claim.");
            }

            string? filePath = null;
            if (createPostDto.File != null && createPostDto.File.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(createPostDto.File.FileName);
                filePath = Path.Combine("uploads", "posts", fileName);

                var fileDirectory = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "posts");
                if (!Directory.Exists(fileDirectory))
                {
                    Directory.CreateDirectory(fileDirectory);
                }

                var fullPath = Path.Combine(fileDirectory, fileName);
                await using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await createPostDto.File.CopyToAsync(stream);
                }
            }

            var post = new Post
            {
                UserId = userId,
                Content = createPostDto.Content,
                Title = createPostDto.Title,
                IsActive = true,
                PostDate = DateTime.UtcNow,
                FilePath = filePath // Salva solo il nome del file nel DB, non il percorso completo
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return Ok(new { post.PostId, post.Title, post.Content, post.FilePath });
        }



        [Authorize]
        [HttpPost("{postId}/comment")]
        public IActionResult CommentOnPost(int postId, [FromBody] CreateCommentDto createCommentDto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var user = _context.Utentis.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            var comment = new Comment
            {
                PostId = postId,
                UserId = userId,
                Content = createCommentDto.Content,
                IsActive = true,
                CommentDate = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            _context.SaveChanges();

            var commentDto = new CommentDto
            {
                CommentId = comment.CommentId,
                PostId = comment.PostId,
                Content = comment.Content,
                CommentDate = comment.CommentDate,
                UserId = comment.UserId,
                LikeCount = 0,
                Likes = new List<LikeDto>(),
                UserName = user.Username,
                UserImage = user.ImageUrl
            };

            return Ok(commentDto);
        }


        [Authorize]
        [HttpPost("like/post/{postId}")]
        public async Task<IActionResult> LikePost(int postId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Verifica se l'utente ha già messo mi piace al post
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (existingLike != null)
            {
                // Se l'utente ha già messo "mi piace", rimuovilo (toggle like)
                _context.Likes.Remove(existingLike);

                // Aggiorna il contatore dei mi piace del post
                var post = await _context.Posts.FindAsync(postId);
                if (post != null)
                {
                    post.Likes -= 1;
                    _context.Entry(post).Property("Likes").IsModified = true;
                }
            }
            else
            {
                // Altrimenti, aggiungi un nuovo "mi piace"
                var like = new Like
                {
                    PostId = postId,
                    UserId = userId
                };

                _context.Likes.Add(like);

                // Aggiorna il contatore dei mi piace del post
                var post = await _context.Posts.FindAsync(postId);
                if (post != null)
                {
                    post.Likes += 1;
                    _context.Entry(post).Property("Likes").IsModified = true;
                }
            }

            await _context.SaveChangesAsync();

            bool isLiked = existingLike == null; // Se existingLike era null, significa che ho aggiunto un mi piace
            return Ok(new { Message = isLiked ? "Like added" : "Like removed" });
        }

        [Authorize]
        [HttpPost("like/comment/{commentId}")]
        public async Task<IActionResult> LikeComment(int commentId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Verifica se il commento esiste
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                return NotFound(new { Message = $"Commento con ID {commentId} non trovato." });
            }

            // Verifica se l'utente ha già messo mi piace al commento
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.CommentId == commentId && l.UserId == userId);

            if (existingLike != null)
            {
                // Se l'utente ha già messo "mi piace", rimuovilo (toggle like)
                _context.Likes.Remove(existingLike);
            }
            else
            {
                // Altrimenti, aggiungi un nuovo "mi piace"
                var like = new Like
                {
                    CommentId = commentId,
                    UserId = userId
                };
                _context.Likes.Add(like);
            }

            // Salva le modifiche nel database
            await _context.SaveChangesAsync();

            var commentWithLikes = await _context.Comments
             .Where(c => c.CommentId == commentId)
             .Select(c => new
             {
                 c.CommentId,
                 LikesCount = c.Likes.Count,
                 LikesDetails = c.Likes.Select(l => new
                 {
                     l.LikeId,
                     l.UserId,
                     User = new
                     {
                         l.User.Username,
                         l.User.ImageUrl
                     }
                 }).ToList()
             }).FirstOrDefaultAsync();

            return Ok(commentWithLikes);

        }
    }
}
