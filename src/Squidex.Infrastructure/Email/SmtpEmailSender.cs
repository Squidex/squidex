// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Squidex.Infrastructure.Email
{
    public sealed class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpClient smtpClient;
        private readonly string sender;

        public SmtpEmailSender(IOptions<SmptOptions> options)
        {
            Guard.NotNull(options, nameof(options));

            var config = options.Value;

            smtpClient = new SmtpClient(config.Server, config.Port)
            {
                Credentials = new NetworkCredential(
                    config.Username,
                    config.Password),
                EnableSsl = config.EnableSsl
            };

            sender = config.Sender;
        }

        public Task SendAsync(string recipient, string subject, string body)
        {
            return smtpClient.SendMailAsync(sender, recipient, subject, body);
        }
    }
}
