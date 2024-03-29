namespace LoginTamplate.Model.Dto.Abbonamento
{
    public class AbbonamentoUpdateDTO
    {
        public decimal? Prezzo { get; set; }
        public int? Quantita { get; set; }
        public string TipoAbbonamento { get; set; }
        public string Descrizione { get; set; }
        public string ImageUrl { get; set; }
    }
}
