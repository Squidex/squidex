﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;
using MailKit.Net.Smtp;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace Squidex.Infrastructure.Email;

[ExcludeFromCodeCoverage]
public sealed class SmtpEmailSender(IOptions<SmtpOptions> options) : IEmailSender
{
    private readonly SmtpOptions options = options.Value;
    private readonly ObjectPool<SmtpClient> clientPool = new DefaultObjectPoolProvider().Create(new DefaultPooledObjectPolicy<SmtpClient>());

    public async Task SendAsync(string recipient, string subject, string body,
        CancellationToken ct = default)
    {
        var smtpClient = clientPool.Get();
        try
        {
            using (var combined = CancellationTokenSource.CreateLinkedTokenSource(ct))
            {
                // Enforce a hard timeout from the configuration.
                combined.CancelAfter(options.Timeout);

                await EnsureConnectedAsync(smtpClient, combined.Token);

                var smtpMessage = new MimeMessage();

                smtpMessage.From.Add(MailboxAddress.Parse(
                    options.Sender));

                smtpMessage.To.Add(MailboxAddress.Parse(
                    recipient));

                smtpMessage.Body = new TextPart(TextFormat.Html)
                {
                    Text = body,
                };

                smtpMessage.Subject = subject;

                await smtpClient.SendAsync(smtpMessage, ct);
            }
        }
        finally
        {
            clientPool.Return(smtpClient);
        }
    }

    private async Task EnsureConnectedAsync(SmtpClient smtpClient,
        CancellationToken ct)
    {
        if (!smtpClient.IsConnected)
        {
            await smtpClient.ConnectAsync(options.Server, options.Port, cancellationToken: ct);
        }

        if (!smtpClient.IsAuthenticated && options.IsAuthenticating())
        {
            await smtpClient.AuthenticateAsync(options.Username, options.Password, ct);
        }
    }
}
