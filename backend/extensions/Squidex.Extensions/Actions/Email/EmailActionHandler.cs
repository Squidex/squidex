// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Extensions.Actions.Email;

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

    protected override async Task<Result> ExecuteJobAsync(EmailJob job,
        CancellationToken ct = default)
    {
        using (var smtpClient = new SmtpClient())
        {
            await smtpClient.ConnectAsync(job.ServerHost, job.ServerPort, cancellationToken: ct);

            await smtpClient.AuthenticateAsync(job.ServerUsername, job.ServerPassword, ct);

            var smtpMessage = new MimeMessage();

            smtpMessage.From.Add(MailboxAddress.Parse(
                job.MessageFrom));

            smtpMessage.To.Add(MailboxAddress.Parse(
                job.MessageTo));

            smtpMessage.Body = new TextPart(TextFormat.Html)
            {
                Text = job.MessageBody
            };

            smtpMessage.Subject = job.MessageSubject;

            await smtpClient.SendAsync(smtpMessage, ct);
        }

        return Result.Complete();
    }
}

public sealed class EmailJob
{
    public int ServerPort { get; set; }

    public string ServerHost { get; set; }

    public string ServerUsername { get; set; }

    public string ServerPassword { get; set; }

    public string MessageFrom { get; set; }

    public string MessageTo { get; set; }

    public string MessageSubject { get; set; }

    public string MessageBody { get; set; }
}
