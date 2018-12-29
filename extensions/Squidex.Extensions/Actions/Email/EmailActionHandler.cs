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

        private readonly EmailOptions emailOptions;

        public EmailActionHandler(RuleEventFormatter formatter, IOptions<EmailOptions> emailOptions)
            : base(formatter)
        {
            Guard.NotNull(emailOptions, nameof(emailOptions));
            this.emailOptions = emailOptions.Value;
        }

        protected override (string Description, EmailJob Data) CreateJob(EnrichedEvent @event, EmailAction action)
        {
            var ruleJob = new EmailJob
            {
                From = action.From,
                To = action.To,
                Subject = Format(action.Subject, @event),
                Body = Format(action.Body, @event)
            };
            return (Description, ruleJob);
        }

        protected override async Task<(string Dump, Exception Exception)> ExecuteJobAsync(EmailJob job)
        {
            using (SmtpClient client = new SmtpClient(emailOptions.Host, emailOptions.Port))
            {
                client.EnableSsl = emailOptions.EnableSsl;
                client.Credentials = new NetworkCredential(emailOptions.Username, emailOptions.Password);
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
        public string From { get; set; }

        public string To { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }
    }
}
