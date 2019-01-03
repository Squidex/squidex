// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;

namespace Squidex.Extensions.Actions.Email
{
    public sealed class EmailActionHandler : RuleActionHandler<EmailAction, EmailJob>
    {
        public EmailActionHandler(RuleEventFormatter formatter)
            : base(formatter)
        {
        }

        protected override (string Description, EmailJob Data) CreateJob(EnrichedEvent @event, EmailAction action)
        {
            var ruleJob = new EmailJob
            {
                ServerHost = action.ServerHost,
                ServerUseSsl = action.ServerUseSsl,
                ServerPassword = action.ServerPassword,
                ServerPort = action.ServerPort,
                ServerUsername = Format(action.ServerUsername, @event),
                MessageFrom = Format(action.MessageFrom, @event),
                MessageTo = Format(action.MessageTo, @event),
                MessageSubject = Format(action.MessageSubject, @event),
                MessageBody = Format(action.MessageBody, @event)
            };

            var description = $"Send an email to {action.MessageTo}";

            return (description, ruleJob);
        }

        protected override async Task<(string Dump, Exception Exception)> ExecuteJobAsync(EmailJob job)
        {
            using (var client = new SmtpClient(job.ServerHost, job.ServerPort))
            {
                client.EnableSsl = job.ServerUseSsl;
                client.Credentials = new NetworkCredential(job.ServerUsername, job.ServerPassword);

                using (var message = new MailMessage(job.MessageFrom, job.MessageTo))
                {
                    message.Subject = job.MessageSubject;
                    message.Body = job.MessageBody;

                    await client.SendMailAsync(message);
                }
            }

            return ("Completed", null);
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
