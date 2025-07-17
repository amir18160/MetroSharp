namespace Application.Interfaces
{
    public interface IEmailService
    {
        Task<bool> SendConfirmationEmail(string to, string UserName, string confirmationLink);
        Task<bool> SendPasswordResetEmail(string to, string userName, string resetLink);
    }
}