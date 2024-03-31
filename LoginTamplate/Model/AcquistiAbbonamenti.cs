using System;
using System.Collections.Generic;

namespace LoginTamplate.Model;

public partial class AcquistiAbbonamenti
{
    public int AcquistoId { get; set; }

    public int? UserId { get; set; }

    public int? Idprodotto { get; set; }

    public DateTime? DataAcquisto { get; set; }

    public DateTime? DataScadenza { get; set; }

    public virtual Abbonamenti? IdprodottoNavigation { get; set; }

    public virtual Utenti? User { get; set; }
}
