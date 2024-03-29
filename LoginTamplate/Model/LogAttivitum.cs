using System;
using System.Collections.Generic;

namespace LoginTamplate.Model;

public partial class LogAttivitum
{
    public int LogId { get; set; }

    public int UserId { get; set; }

    public DateTime Timestamp { get; set; }

    public string Azione { get; set; } = null!;

    public virtual Utenti User { get; set; } = null!;
}
