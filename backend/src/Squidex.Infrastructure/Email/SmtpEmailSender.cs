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
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;

namespace Squidex.Infrastructure.Email
{
    [ExcludeFromCodeCoverage]
    public sealed class SmtpEmailSender : IEmailSender
    {
        private readonly SmptOptions options;
        private readonly ObjectPool<SmtpClient> clientPool;

        internal sealed class SmtpClientPolicy : PooledObjectPolicy<SmtpClient>
        {
            private readonly SmptOptions options;

            public SmtpClientPolicy(SmptOptions options)
            {
                this.options = options;
            }

            public override SmtpClient Create()
            {
                return new SmtpClient(options.Server, options.Port)
                {
                    Credentials = new NetworkCredential(
                        options.Username,
                        options.Password),

                    EnableSsl = options.EnableSsl
                };
            }

            public override bool Return(SmtpClient obj)
            {
                return true;
            }
        }

        public SmtpEmailSender(IOptions<SmptOptions> options)
        {
            Guard.NotNull(options);

            this.options = options.Value;

            clientPool = new DefaultObjectPoolProvider().Create(new SmtpClientPolicy(options.Value));
        }

        public async Task SendAsync(string recipient, string subject, string body)
        {
            var smtpClient = clientPool.Get();
            try
            {
                await smtpClient.SendMailAsync(options.Sender, recipient, subject, body);
            }
            finally
            {
                clientPool.Return(smtpClient);
            }
        }
    }
}
