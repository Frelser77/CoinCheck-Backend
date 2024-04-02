namespace LoginTamplate.Model.Dto.Utente
{
    public class PreferenzeUtenteDto
    {
        public int CriptoId { get; set; }

        public string NomeCoin { get; set; }

        public string SimboloCoin { get; set; }

        public decimal? PrezzoUsd { get; set; }

        public decimal? Variazione24h { get; set; }

        public decimal? Volume24h { get; set; }
        public bool? NotificaSogliaPrezzo { get; set; }
        public decimal? SogliaPrezzo { get; set; }
    }
}
