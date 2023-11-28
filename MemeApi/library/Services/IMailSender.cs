using System.Net.Mail;

namespace MemeApi.library.Services;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
public interface IMailSender
{
    public bool sendNoReplyMail(MailAddress recipient, string subject, string body);
    public bool sendMemeOfTheDayMail(MailAddress recipient, byte[] memeOfTheDay);
}
