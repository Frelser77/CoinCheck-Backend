namespace LoginTamplate.Model.ViewModel
{
    public class CreateCheckoutSessionRequest
    {
        public string UserId { get; set; }
        public string IdProdotto { get; set; }
        public List<CartItem> Items { get; set; }
    }
}
