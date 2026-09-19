using FoodMart.Settings;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Net.Mail;

namespace FoodMart.Services.EmailServices
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(
            IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendDiscountCodeAsync(
            string fullName,
            string email,
            string discountCode,
            int discountRate,
            DateTime expiresAt)
        {
            if (string.IsNullOrWhiteSpace(_emailSettings.SmtpServer) ||
                _emailSettings.SmtpPort <= 0 ||
                string.IsNullOrWhiteSpace(_emailSettings.SenderEmail) ||
                string.IsNullOrWhiteSpace(_emailSettings.Username) ||
                string.IsNullOrWhiteSpace(_emailSettings.Password))
            {
                throw new InvalidOperationException("E-posta gönderimi için SMTP ayarları eksik.");
            }

            var message = new MimeMessage();

            message.From.Add(
                new MailboxAddress(
                    _emailSettings.SenderName,
                    _emailSettings.SenderEmail));

            message.To.Add(
                MailboxAddress.Parse(email));

            message.Subject =
                $"FoodMarkt %{discountRate} İndirim Kodunuz";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $"""
                    <!DOCTYPE html>
                    <html lang="tr">
                    <head>
                        <meta charset="UTF-8">
                    </head>

                    <body style="
                        margin:0;
                        padding:0;
                        background:#f4f7f4;
                        font-family:Arial,Helvetica,sans-serif;
                        color:#26332a;">

                        <div style="
                            max-width:600px;
                            margin:40px auto;
                            background:#ffffff;
                            border-radius:16px;
                            overflow:hidden;
                            box-shadow:0 5px 25px rgba(0,0,0,.06);">

                            <div style="
                                background:#173c24;
                                padding:30px;
                                text-align:center;
                                color:white;">

                                <h1 style="
                                    margin:0;
                                    font-size:28px;">
                                    FoodMarkt
                                </h1>

                                <p style="
                                    margin:8px 0 0;
                                    opacity:.8;">
                                    Alışverişiniz daha avantajlı
                                </p>

                            </div>


                            <div style="padding:40px 35px;">

                                <h2 style="margin-top:0;">
                                    Merhaba {System.Net.WebUtility.HtmlEncode(fullName)},
                                </h2>

                                <p style="
                                    color:#68736b;
                                    line-height:1.7;">

                                    FoodMarkt kampanyasına katıldığınız
                                    için teşekkür ederiz.

                                    İlk alışverişinizde kullanabileceğiniz
                                    %{discountRate} indirim kodunuz hazır.

                                </p>


                                <div style="
                                    background:#f1f8f2;
                                    border:2px dashed #4b9b59;
                                    border-radius:12px;
                                    padding:25px;
                                    margin:30px 0;
                                    text-align:center;">

                                    <span style="
                                        display:block;
                                        color:#718076;
                                        font-size:13px;
                                        margin-bottom:10px;">

                                        İNDİRİM KODUNUZ

                                    </span>

                                    <strong style="
                                        display:block;
                                        color:#348a48;
                                        font-size:28px;
                                        letter-spacing:3px;">

                                        {discountCode}

                                    </strong>

                                    <span style="
                                        display:block;
                                        margin-top:12px;
                                        font-size:18px;
                                        font-weight:bold;">

                                        %{discountRate} İndirim

                                    </span>

                                </div>


                                <p style="
                                    color:#68736b;
                                    font-size:14px;">

                                    Son Kullanma Tarihi:
                                    <strong>
                                        {expiresAt.ToLocalTime():dd.MM.yyyy}
                                    </strong>

                                </p>


                                <p style="
                                    color:#9aa29c;
                                    font-size:12px;
                                    margin-top:35px;">

                                    Bu indirim kodu size özel olarak
                                    oluşturulmuştur.

                                </p>

                            </div>


                            <div style="
                                padding:20px;
                                background:#f7f9f7;
                                text-align:center;
                                color:#929b94;
                                font-size:12px;">

                                © {DateTime.Now.Year} FoodMarkt

                            </div>

                        </div>

                    </body>
                    </html>
                    """
            };

            message.Body = bodyBuilder.ToMessageBody();

            using var smtpClient = new MailKit.Net.Smtp.SmtpClient();

            await smtpClient.ConnectAsync(
                _emailSettings.SmtpServer,
                _emailSettings.SmtpPort,
                SecureSocketOptions.StartTls);

            await smtpClient.AuthenticateAsync(
                _emailSettings.Username,
                _emailSettings.Password);

            await smtpClient.SendAsync(message);

            await smtpClient.DisconnectAsync(true);
        }
    }
}