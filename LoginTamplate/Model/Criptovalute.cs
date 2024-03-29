using System;
using System.Collections.Generic;

namespace LoginTamplate.Model;

public partial class Criptovalute
{
    public int CriptoId { get; set; }

    public string Nome { get; set; } = null!;

    public string Simbolo { get; set; } = null!;

    public int? Rank { get; set; }

    public decimal? PrezzoUsd { get; set; }

    public decimal? Variazione24h { get; set; }

    public decimal? Volume24h { get; set; }

    public decimal? MarketCap { get; set; }

    public DateTime? UltimoAggiornamento { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<PreferenzeUtente> PreferenzeUtentes { get; set; } = new List<PreferenzeUtente>();
}
