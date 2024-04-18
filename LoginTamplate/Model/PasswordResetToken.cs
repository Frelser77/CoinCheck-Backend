using System;
using System.Collections.Generic;

namespace LoginTamplate.Model;

public partial class PasswordResetToken
{
    public int TokenId { get; set; }

    public int UserId { get; set; }

    public string Token { get; set; } = null!;

    public DateTime ExpiryDate { get; set; }

    public virtual Utenti User { get; set; } = null!;
}
