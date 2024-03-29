using System.ComponentModel.DataAnnotations;

namespace LoginTamplate.Model.Dto.Criptovaluta
{
    public class PreferenzaDto
    {
        public int PreferenzaId { get; set; }
        public int UserId { get; set; }

        public int CriptoId { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string? CriptoNome { get; set; }
    }
}
