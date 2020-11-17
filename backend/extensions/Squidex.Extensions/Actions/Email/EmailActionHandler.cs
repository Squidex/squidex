// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;

namespace Squidex.Extensions.Actions.Email
{
    public sealed class EmailActionHandler : RuleActionHandler<EmailAction, EmailJob>
    {
        public EmailActionHandler(RuleEventFormatter formatter)
            : base(formatter)
        {
        }

        protected override async Task<(string Description, EmailJob Data)> CreateJobAsync(EnrichedEvent @event, EmailAction action)
        {
            var ruleJob = new EmailJob
            {
                ServerHost = action.ServerHost,
                ServerUseSsl = action.ServerUseSsl,
                ServerPassword = action.ServerPassword,
                ServerPort = action.ServerPort,
                ServerUsername = await FormatAsync(action.ServerUsername, @event),
                MessageFrom = await FormatAsync(action.MessageFrom, @event),
                MessageTo = await FormatAsync(action.MessageTo, @event),
                MessageSubject = await FormatAsync(action.MessageSubject, @event),
                MessageBody = await FormatAsync(action.MessageBody, @event)
            };

            var description = $"Send an email to {action.MessageTo}";

            return (description, ruleJob);
        }

        protected override async Task<Result> ExecuteJobAsync(EmailJob job, CancellationToken ct = default)
        {
            await CheckConnectionAsync(job, ct);

            using (var client = new SmtpClient(job.ServerHost, job.ServerPort)
            {
                Credentials = new NetworkCredential(
                    job.ServerUsername,
                    job.ServerPassword),

                EnableSsl = job.ServerUseSsl
            })
            {
                using (ct.Register(client.SendAsyncCancel))
                {
                    await client.SendMailAsync(
                        job.MessageFrom,
                        job.MessageTo,
                        job.MessageSubject,
                        job.MessageBody,
                        ct);
                }
            }

            return Result.Complete();
        }

        private static async Task CheckConnectionAsync(EmailJob job, CancellationToken ct)
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                var tcs = new TaskCompletionSource<IAsyncResult>();

                socket.BeginConnect(job.ServerHost, job.ServerPort, tcs.SetResult, null);

                using (ct.Register(() =>
                {
                    tcs.TrySetException(new OperationCanceledException($"Failed to establish a connection to {job.ServerHost}:{job.ServerPort}"));
                }))
                {
                    await tcs.Task;
                }
            }
        }
    }

    public sealed class EmailJob
    {
        public int ServerPort { get; set; }

        public string ServerHost { get; set; }

        public string ServerUsername { get; set; }

        public string ServerPassword { get; set; }

        public bool ServerUseSsl { get; set; }

        public string MessageFrom { get; set; }

        public string MessageTo { get; set; }

        public string MessageSubject { get; set; }

        public string MessageBody { get; set; }
    }
}
