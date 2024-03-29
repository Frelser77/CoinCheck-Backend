namespace LoginTamplate.Model.Dto.Criptovaluta
{
    public class CriptovalutaDto
    {
        public string Nome { get; set; }
        public string Simbolo { get; set; }
        public decimal? PrezzoUsd { get; set; }
        public decimal? Variazione24h { get; set; }
        public DateTime? UltimoAggiornamento { get; set; }

        public decimal? Volume24h { get; set; }
    }
}
