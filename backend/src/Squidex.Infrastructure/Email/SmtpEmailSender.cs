// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace Squidex.Infrastructure.Email
{
    [ExcludeFromCodeCoverage]
    public sealed class SmtpEmailSender : IEmailSender
    {
        private readonly SmptOptions options;

        public SmtpEmailSender(IOptions<SmptOptions> options)
        {
            Guard.NotNull(options);

            this.options = options.Value;
        }

        public async Task SendAsync(string recipient, string subject, string body)
        {
            using (var smtpClient = new SmtpClient(options.Server, options.Port)
            {
                Credentials = new NetworkCredential(
                    options.Username,
                    options.Password),

                EnableSsl = options.EnableSsl
            })
            {
                await smtpClient.SendMailAsync(options.Sender, recipient, subject, body);
            }
        }
    }
}
