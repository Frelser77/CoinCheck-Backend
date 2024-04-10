using LoginTamplate.Model.Dto.Email;

namespace LoginTamplate.Data
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailDto emailDto);
    }

}
