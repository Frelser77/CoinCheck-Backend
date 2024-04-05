using System;
using System.Collections.Generic;

namespace LoginTamplate.Model;

public partial class Post
{
    public int PostId { get; set; }

    public int UserId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime PostDate { get; set; }

    public bool IsActive { get; set; }

    public string Title { get; set; } = null!;

    public string? FilePath { get; set; }

    public int? Likes { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Like> LikesNavigation { get; set; } = new List<Like>();

    public virtual Utenti User { get; set; } = null!;
}
