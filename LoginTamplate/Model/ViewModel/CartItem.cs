namespace LoginTamplate.Model.ViewModel
{
    public class CartItem
    {
        public int IdProdotto { get; set; }
        public double Prezzo { get; set; }
        public int Quantita { get; set; }
        public string TipoAbbonamento { get; set; }
        public string Descrizione { get; set; }

        public string ImageUrl { get; set; }
    }
}
