using MemeApi.Models.Entity;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace MemeApi.library.Services;

public class MailSender : IMailSender
{
    private readonly IConfiguration _configuration;
    public MailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool sendNoReplyMail(MailAddress recipient, string subject, string body)
    {
        var client = SmtpClient(_configuration["Email.NoReply.Mail"], _configuration["Email.NoReply.Password"]);
        var fromAddress = new MailAddress(_configuration["Email.NoReply.Mail"]);

        var message = new MailMessage(fromAddress, recipient)
        {
            Subject = subject,
            Body = body
        };

        return sendMail(client, message);
    }

    public bool sendMemeOfTheDayMail(MailAddress recipient, byte[] memeOfTheDay)
    {
        var client = SmtpClient(_configuration["Email.MemeOfTheDay.Mail"], _configuration["Email.MemeOfTheDay.Password"]);
        var fromAddress = new MailAddress(_configuration["Email.MemeOfTheDay.Mail"]);

        Attachment att = new Attachment(new MemoryStream(memeOfTheDay), "gaming.png");

        //string htmlBody = @"<img src='data:image/png;base64," + Convert.ToBase64String(memeOfTheDay, 0, memeOfTheDay.Length) + @"'/>";
        string htmlBody = @"<img src='cid:MemeOfTheDay.png'/>";

        var message = new MailMessage(fromAddress, recipient)
        {
            Subject = "Meme of the day: " + DateTime.UtcNow.Date.ToString(),
            IsBodyHtml = true
        };

        message.Attachments.Add(att);
        message.Attachments[0].ContentId = "MemeOfTheDay.png";
        message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(htmlBody, null, "text/html"));

        return sendMail(client, message);
    }

    private SmtpClient SmtpClient(string mail, string password)
    {
        return new SmtpClient
        {
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Host = _configuration["Email.Host"],
            Port = int.Parse(_configuration["Email.Host.Port"]),
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(mail, password)
        };
    }

    private bool sendMail(SmtpClient client, MailMessage message)
    {
        try
        {
            client.Send(message);
            return true;
        }
        catch (Exception)
        {
            return false;
        }   
    }
}
