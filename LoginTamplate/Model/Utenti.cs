using System;
using System.Collections.Generic;

namespace LoginTamplate.Model;

public partial class Utenti
{
    public int UserId { get; set; }

    public string Username { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int RuoloId { get; set; }

    public bool IsActive { get; set; }

    public string? ImageUrl { get; set; }

    public int? Idabbonamento { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual Abbonamenti? IdabbonamentoNavigation { get; set; }

    public virtual ICollection<LogAttivitum> LogAttivita { get; set; } = new List<LogAttivitum>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual ICollection<PreferenzeUtente> PreferenzeUtentes { get; set; } = new List<PreferenzeUtente>();

    public virtual Ruoli Ruolo { get; set; } = null!;
}
