namespace LoginTamplate.Model;

public partial class Abbonamenti
{
    public int Idprodotto { get; set; }

    public decimal Prezzo { get; set; }

    public int? Quantita { get; set; }

    public string? TipoAbbonamento { get; set; }

    public string? Descrizione { get; set; }

    public string? ImageUrl { get; set; }

    public virtual ICollection<AcquistiAbbonamenti> AcquistiAbbonamentis { get; set; } = new List<AcquistiAbbonamenti>();

    public virtual ICollection<Utenti> Utentis { get; set; } = new List<Utenti>();
}
