using LibraryAPI.Models;

namespace LibraryAPI.Utils
{
    public interface IEmailService
    {
        void SendMail(Email email);
    }
}
