using System;
using System.Collections.Generic;

namespace LoginTamplate.Model;

public partial class Ruoli
{
    public int RuoloId { get; set; }

    public string NomeRuolo { get; set; } = null!;

    public virtual ICollection<Utenti> Utentis { get; set; } = new List<Utenti>();
}
