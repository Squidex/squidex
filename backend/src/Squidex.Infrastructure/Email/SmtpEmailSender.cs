// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;

namespace Squidex.Infrastructure.Email
{
    [ExcludeFromCodeCoverage]
    public sealed class SmtpEmailSender : IEmailSender
    {
        private readonly SmtpOptions options;
        private readonly ObjectPool<SmtpClient> clientPool;

        internal sealed class SmtpClientPolicy : PooledObjectPolicy<SmtpClient>
        {
            private readonly SmtpOptions options;

            public SmtpClientPolicy(SmtpOptions options)
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

                    EnableSsl = options.EnableSsl,

                    Timeout = options.Timeout
                };
            }

            public override bool Return(SmtpClient obj)
            {
                return true;
            }
        }

        public SmtpEmailSender(IOptions<SmtpOptions> options)
        {
            Guard.NotNull(options, nameof(options));

            this.options = options.Value;

            clientPool = new DefaultObjectPoolProvider().Create(new SmtpClientPolicy(options.Value));
        }

        public async Task SendAsync(string recipient, string subject, string body)
        {
            var smtpClient = clientPool.Get();
            try
            {
                using (var cts = new CancellationTokenSource(options.Timeout))
                {
                    await CheckConnectionAsync(cts.Token);

                    using (cts.Token.Register(smtpClient.SendAsyncCancel))
                    {
                        await smtpClient.SendMailAsync(options.Sender, recipient, subject, body);
                    }
                }
            }
            finally
            {
                clientPool.Return(smtpClient);
            }
        }

        private async Task CheckConnectionAsync(CancellationToken ct)
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                var tcs = new TaskCompletionSource<IAsyncResult>();

                socket.BeginConnect(options.Server, options.Port, tcs.SetResult, null);

                using (ct.Register(() =>
                {
                    tcs.TrySetException(new OperationCanceledException($"Failed to establish a connection to {options.Server}:{options.Port}"));
                }))
                {
                    await tcs.Task;
                }
            }
        }
    }
}
