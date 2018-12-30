// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Infrastructure;

namespace Squidex.Extensions.Actions.Email
{
    public sealed class EmailActionHandler : RuleActionHandler<EmailAction, EmailJob>
    {
        private const string Description = "Send an Email";

        public EmailActionHandler(RuleEventFormatter formatter)
            : base(formatter)
        {
        }

        protected override (string Description, EmailJob Data) CreateJob(EnrichedEvent @event, EmailAction action)
        {
            var ruleJob = new EmailJob
            {
                Host = action.Host,
                EnableSsl = action.EnableSsl,
                Password = action.Password,
                Port = action.Port,
                Username = Format(action.Username, @event),
                From = Format(action.From, @event),
                To = Format(action.To, @event),
                Subject = Format(action.Subject, @event),
                Body = Format(action.Body, @event)
            };

            return (Description, ruleJob);
        }

        protected override async Task<(string Dump, Exception Exception)> ExecuteJobAsync(EmailJob job)
        {
            using (var client = new SmtpClient(job.Host, job.Port))
            {
                client.EnableSsl = job.EnableSsl;
                client.Credentials = new NetworkCredential(job.Username, job.Password);

                using (var message = new MailMessage(job.From, job.To))
                {
                    message.Subject = job.Subject;
                    message.Body = job.Body;
                    await client.SendMailAsync(message);
                }
            }

            return ("Completed", null);
        }
    }

    public class EmailJob
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public bool EnableSsl { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }
    }
}
