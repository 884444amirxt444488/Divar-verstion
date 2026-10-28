
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

public class EmailServices
{
    private readonly IConfiguration configuration;

    public EmailServices(IConfiguration configuration)
    {
        this.configuration = configuration;
    }


    public async Task SendMail(string Code, string Email)
    {
        var Message = new MimeMessage();

        string Username = configuration["EmailService:Username"] ?? throw new InvalidOperationException("Username not found");

        string Password = configuration["EmailService:Password"] ?? throw new InvalidOperationException("Password not found");

        Message.From.Add(new MailboxAddress("Shop", Username));
        Message.To.Add(MailboxAddress.Parse(Email));

        Message.Subject = "Code to change your password";

        Message.Body = new TextPart("html")
        {
            Text = $"""
                <div style="background-color: black;
                    padding: 15px;
                    color: white;
                    border-radius: 20px;
                    width: 80%;
                    max-width: 400px;
                    min-width: 80px;
                    margin: 0 auto;
                    border: 2px solid rgb(255, 0, 0);"
                >
                    <h2 style="text-align: center;
                        background: linear-gradient(
                            20deg,
                            rgb(145, 145, 0),
                            rgb(0, 0, 124),
                            rgb(116, 1, 1),
                            rgb(109, 108, 108),
                            black
                        );
                        padding: 10px;
                        border-radius: 20px;
                        box-shadow: 0 0 15px rgb(255, 255, 255);"
                    >Password Reset Request</h2>
                    <p>Hello. its shop.</p>
                    <p>
                        We received a request to reset your password.
                        Please use the verification code below to continue.
                    </p>
                    <h3>****{Code}****</h3>
                    <p>
                        This code will expire in 10 minutes.
                        Please do not share it with anyone.
                    </p>
                    <p>
                        If you did not request a password reset, you can safely ignore this email.
                        Your password will remain unchanged.
                    </p>
                    <p>
                        Best regards,<br>
                        The ShopApi Team
                    </p>
                </div>
            
            
            """
        };

        using var client = new SmtpClient();

        await client.ConnectAsync(
            "smtp.gmail.com",
            587,
            SecureSocketOptions.StartTls
        );
        
        await client.AuthenticateAsync(Username, Password);
        await client.SendAsync(Message);
        await client.DisconnectAsync(true);
    }
}
