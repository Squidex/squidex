// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using SendGrid;
using SendGrid.Helpers.Mail;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;

namespace Squidex.Extensions.Actions.SendGrid
{
    public sealed class SendGridActionHandler : RuleActionHandler<SendGridAction, SendGridJob>
    {
        public SendGridActionHandler(RuleEventFormatter formatter)
            : base(formatter)
        {
        }

        protected override (string Description, SendGridJob Data) CreateJob(EnrichedEvent @event, SendGridAction action)
        {
            var ruleJob = new SendGridJob
            {
                APIKey = action.APIKey,
                MessageFrom = Format(action.MessageFrom, @event),
                MessageTo = Format(action.MessageTo, @event),
                MessageSubject = Format(action.MessageSubject, @event),
                MessageBody = Format(action.MessageBody, @event)
            };

            var description = $"Send an email to {action.MessageTo}";

            return (description, ruleJob);
        }

        protected override async Task<Result> ExecuteJobAsync(SendGridJob job, CancellationToken ct = default)
        {
            var client = new SendGridClient(job.APIKey);
            var from = new EmailAddress(job.MessageFrom);
            var subject = job.MessageSubject;
            var to = new EmailAddress(job.MessageTo);
            var htmlContent = job.MessageBody;
            var email = MailHelper.CreateSingleEmail(from, to, subject, string.Empty, htmlContent);
            await client.SendEmailAsync(email);
            return Result.Complete();
        }
    }

    public sealed class SendGridJob
    {
        public string APIKey { get; set; }

        public string MessageFrom { get; set; }

        public string MessageTo { get; set; }

        public string MessageSubject { get; set; }

        public string MessageBody { get; set; }
    }
}
