using LoginTamplate.Model;
using LoginTamplate.Model.Dto.Comment;
using LoginTamplate.Model.Dto.Post;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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
        public IActionResult GetAllPosts([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = 10)
        {
            // Calcola il numero di post da saltare
            int skip = pageIndex * pageSize;

            // Ottieni il totale dei post attivi
            int totalPosts = _context.Posts.Count(p => p.IsActive);

            var postEntities = _context.Posts
             .Include(p => p.LikesNavigation).ThenInclude(l => l.User)
             .Include(p => p.User).ThenInclude(u => u.Ruolo)
             .Include(p => p.Comments).ThenInclude(c => c.User)
             .Include(p => p.Comments).ThenInclude(c => c.Likes).ThenInclude(l => l.User)
             .Where(p => p.IsActive)
             .OrderByDescending(p => p.PostDate)
             .Skip(skip)
             .Take(pageSize)
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
                    UserImage = c.User.ImageUrl,
                    RoleName = c.User.Ruolo?.NomeRuolo
                }).ToList() : new List<CommentDto>(),
            }).ToList();

            var response = new
            {
                Total = totalPosts,
                Posts = posts,
                PageIndex = pageIndex,
                PageSize = posts.Count,
            };

            return Ok(response);
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
        public async Task<IActionResult> CreatePost([FromForm] CrudPostDto createPostDto)
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

            // Controlla se almeno uno tra Title, Content e File è fornito
            if (string.IsNullOrEmpty(createPostDto.Title) && string.IsNullOrEmpty(createPostDto.Content) && createPostDto.File == null)
            {
                return BadRequest("Almeno uno tra Title, Content e File deve essere fornito.");
            }

            var post = new Post
            {
                UserId = userId,
                Content = createPostDto.Content ?? "",
                Title = createPostDto.Title ?? "",
                IsActive = true,
                PostDate = DateTime.UtcNow,
                FilePath = filePath
            };

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            return Ok(new { post.PostId, post.Title, post.Content, post.FilePath });
        }



        [Authorize]
        [HttpPost("{postId}/comment")]
        public IActionResult CommentOnPost(int postId, [FromBody] CrudCommentDto createCommentDto)
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
            var updatedPost = await _context.Posts.FindAsync(postId);
            var likeCount = updatedPost?.Likes ?? 0;

            bool isLiked = existingLike == null;
            return Ok(new
            {
                Message = isLiked ? "Like added" : "Like removed",
                LikeCount = likeCount
            });
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

        [Authorize]
        [HttpPut("edit/post/{postId}")]
        public async Task<IActionResult> EditPost(int postId, [FromForm] CrudPostDto editPostDto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var post = await _context.Posts.FindAsync(postId);

            if (post == null)
            {
                return NotFound(new { Message = $"Post con ID {postId} non trovato." });
            }

            if (post.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Moderatore"))
            {
                return Forbid(); // L'utente non ha i permessi per modificare il post
            }

            // Aggiorna i campi forniti
            if (!string.IsNullOrEmpty(editPostDto.Title))
            {
                post.Title = editPostDto.Title;
            }

            if (!string.IsNullOrEmpty(editPostDto.Content))
            {
                post.Content = editPostDto.Content;
            }

            if (editPostDto.File != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(editPostDto.File.FileName);
                var filePath = Path.Combine("uploads", "posts", fileName);

                // Se esiste, elimina il vecchio file
                if (!string.IsNullOrEmpty(post.FilePath))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), post.FilePath);
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Salva il nuovo file sul server
                var fileDirectory = Path.Combine(Directory.GetCurrentDirectory(), "uploads", "posts");
                if (!Directory.Exists(fileDirectory))
                {
                    Directory.CreateDirectory(fileDirectory);
                }

                var fullPath = Path.Combine(fileDirectory, fileName);
                await using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await editPostDto.File.CopyToAsync(stream);
                }
                post.FilePath = filePath;
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Post modificato con successo." });
        }

        [Authorize(Roles = "Admin,Moderatore")]
        [HttpPut("delete/post/{postId}")]
        public async Task<IActionResult> SoftDeletePost(int postId)
        {
            // Verifica l'esistenza del post prima di tentare la soft delete
            var postExists = await _context.Posts.AnyAsync(p => p.PostId == postId);
            if (!postExists)
            {
                return NotFound(new { Message = $"Post con ID {postId} non trovato." });
            }

            // Esegue la stored procedure per la soft delete del post e dei commenti correlati
            var parameter = new SqlParameter("@PostId", postId);
            await _context.Database.ExecuteSqlRawAsync("EXEC dbo.SoftDeletePost @PostId", parameter);

            return Ok(new { Message = "Post eliminato con successo." });
        }


        [Authorize]
        [HttpPut("edit/comment/{commentId}")]
        public async Task<IActionResult> EditComment(int commentId, [FromBody] CrudCommentDto editCommentDto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var comment = await _context.Comments.FindAsync(commentId);

            if (comment == null)
            {
                return NotFound(new { Message = $"Commento con ID {commentId} non trovato." });
            }

            if (comment.UserId != userId && !User.IsInRole("Admin") && !User.IsInRole("Moderatore"))
            {
                return Forbid();
            }

            comment.Content = editCommentDto.Content;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Commento modificato con successo." });
        }

        [Authorize(Roles = "Admin,Moderatore")]
        [HttpPut("delete/comment/{commentId}")]
        public async Task<IActionResult> SoftDeleteComment(int commentId)
        {
            // Verifica l'esistenza del commento prima di tentare la soft delete
            var commentExists = await _context.Comments.AnyAsync(c => c.CommentId == commentId);
            if (!commentExists)
            {
                return NotFound(new { Message = $"Commento con ID {commentId} non trovato." });
            }

            // Esegue la stored procedure per la soft delete del commento
            var parameter = new SqlParameter("@CommentId", commentId);
            await _context.Database.ExecuteSqlRawAsync("EXEC dbo.SoftDeleteComment @CommentId", parameter);

            return Ok(new { Message = "Commento eliminato con successo." });
        }

    }
}
