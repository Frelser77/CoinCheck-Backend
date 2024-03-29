using System;
using System.Collections.Generic;

namespace LoginTamplate.Model;

public partial class Comment
{
    public int CommentId { get; set; }

    public int PostId { get; set; }

    public int UserId { get; set; }

    public string Content { get; set; } = null!;

    public DateTime CommentDate { get; set; }

    public bool IsActive { get; set; }

    public virtual Post Post { get; set; } = null!;

    public virtual Utenti User { get; set; } = null!;
}
