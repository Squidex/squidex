﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;
using Squidex.CLI.Commands.Implementation;
using Squidex.CLI.Commands.Implementation.FileSystem;
using Squidex.CLI.Commands.Implementation.Sync;
using Squidex.CLI.Commands.Implementation.Sync.App;
using Squidex.CLI.Commands.Implementation.Sync.AssetFolders;
using Squidex.CLI.Commands.Implementation.Sync.Assets;
using Squidex.CLI.Commands.Implementation.Sync.Contents;
using Squidex.CLI.Commands.Implementation.Sync.Rules;
using Squidex.CLI.Commands.Implementation.Sync.Schemas;
using Squidex.CLI.Commands.Implementation.Sync.Workflows;
using Squidex.CLI.Configuration;
using Squidex.ClientLibrary;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json;
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.Apps.Templates;

public sealed class TemplateCommandMiddleware(
    TemplatesClient templatesClient,
    IOptions<TemplatesOptions> templateOptions,
    IUrlGenerator urlGenerator,
    IJsonSerializer jsonSerializer,
    ISemanticLog log)
    : ICommandMiddleware
{
    private readonly TemplatesOptions templateOptions = templateOptions.Value;

    public async Task HandleAsync(CommandContext context, NextDelegate next,
        CancellationToken ct)
    {
        await next(context, ct);

        if (context.IsCompleted && context.Command is CreateApp createApp)
        {
            await ApplyTemplateAsync(context.Result<App>(), createApp.Template);
        }
    }

    private async Task ApplyTemplateAsync(App app, string? template)
    {
        if (string.IsNullOrWhiteSpace(template))
        {
            return;
        }

        var repository = await templatesClient.GetRepositoryUrl(template);

        if (string.IsNullOrEmpty(repository))
        {
            log.LogWarning(w => w
                .WriteProperty("message", "Template not found.")
                .WriteProperty("template", template));
            return;
        }

        using (var cliLog = new StringLogger(jsonSerializer))
        {
            try
            {
                var session = CreateSession(app);

                var syncService = await CreateSyncServiceAsync(repository, session);
                var syncOptions = new SyncOptions();

                var targets = new ISynchronizer[]
                {
                    new AppSynchronizer(cliLog),
                    new AssetFoldersSynchronizer(cliLog),
                    new AssetsSynchronizer(cliLog),
                    new RulesSynchronizer(cliLog),
                    new SchemasSynchronizer(cliLog),
                    new WorkflowsSynchronizer(cliLog),
                    new ContentsSynchronizer(cliLog),
                };

                foreach (var target in targets)
                {
                    await target.ImportAsync(syncService, syncOptions, session);
                }
            }
            finally
            {
                cliLog.Flush(log, template);
            }
        }
    }

    private static async Task<ISyncService> CreateSyncServiceAsync(string repository, ISession session)
    {
        var fs = await FileSystems.CreateAsync(repository);

        return new SyncService(fs, session);
    }

    private ISession CreateSession(App app)
    {
        var client = app.Clients.First();

        var url = templateOptions.LocalUrl;

        if (string.IsNullOrEmpty(url))
        {
            url = urlGenerator.Root();
        }

        return new Session(
            new DirectoryInfo(Path.GetTempPath()),
            new SquidexClient(new SquidexOptions
            {
                IgnoreSelfSignedCertificates = true,
                AppName = app.Name,
                ClientId = $"{app.Name}:{client.Key}",
                ClientSecret = client.Value.Secret,
                Url = url,
            }));
    }
}
