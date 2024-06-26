﻿using System.Net;
using System.Net.Mail;

namespace IdentityCodeYad.Tools;

public interface IEmailSender
{   
    Task SendEmailAsync(EmailModel email);
}
public class EmailSender : IEmailSender
{
    public async Task SendEmailAsync(EmailModel email)
    {
        MailMessage mail = new MailMessage()
        {
            From = new MailAddress("tt.reset@gmail.com", "r"),
            To = { email.To },
            Subject = email.Subject,
            Body = email.Body,
            IsBodyHtml = true,
        };
        SmtpClient smtpServer = new SmtpClient("smtp.gmail.com", 587) // Host => forExample smtp.gmail.com
        {
            Credentials = new System.Net.NetworkCredential("MyEmailName@gmail.com", "xx"), // UserName == Email
            EnableSsl = true,
        };
        smtpServer.Send(mail);
        await Task.CompletedTask;
    }
}
/// <summary>
/// Smtp Ports
/// </summary>
// Not-Encrypted 25,
// Secure Tls 587
// Secure SSL 465

public class EmailModel
{
    public EmailModel(string to, string subject, string body)
    {
        To = to;
        Subject = subject;
        Body = body;
    }
    public string To { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
}