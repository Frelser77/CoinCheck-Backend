namespace LoginTamplate.Model;

public partial class PreferenzeUtente
{
    public int PreferenzaId { get; set; }

    public int UserId { get; set; }

    public int CriptoId { get; set; }

    public bool? NotificaSogliaPrezzo { get; set; }

    public decimal? SogliaPrezzo { get; set; }

    public virtual Criptovalute Cripto { get; set; } = null!;

    public virtual Utenti User { get; set; } = null!;
}

