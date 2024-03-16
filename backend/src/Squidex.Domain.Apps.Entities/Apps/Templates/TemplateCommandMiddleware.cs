// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Squidex.CLI.Commands.Implementation;
using Squidex.CLI.Commands.Implementation.FileSystem;
using Squidex.CLI.Commands.Implementation.Sync;
using Squidex.CLI.Commands.Implementation.Sync.App;
using Squidex.CLI.Commands.Implementation.Sync.AssertFolders;
using Squidex.CLI.Commands.Implementation.Sync.Assets;
using Squidex.CLI.Commands.Implementation.Sync.Rules;
using Squidex.CLI.Commands.Implementation.Sync.Schemas;
using Squidex.CLI.Commands.Implementation.Sync.Workflows;
using Squidex.CLI.Configuration;
using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Configuration;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Commands;
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.Apps.Templates
{
    public sealed class TemplateCommandMiddleware : ICommandMiddleware
    {
        private readonly TemplatesClient templatesClient;
        private readonly IUrlGenerator urlGenerator;
        private readonly ISemanticLog log;

        public TemplateCommandMiddleware(TemplatesClient templatesClient, IUrlGenerator urlGenerator,
            ISemanticLog log)
        {
            this.templatesClient = templatesClient;
            this.urlGenerator = urlGenerator;

            this.log = log;
        }

        public async Task HandleAsync(CommandContext context, NextDelegate next)
        {
            await next(context);

            if (context.IsCompleted && context.Command is CreateApp createApp)
            {
                await ApplyTemplateAsync(context.Result<IAppEntity>(), createApp.Template);
            }
        }

        private async Task ApplyTemplateAsync(IAppEntity app, string? template)
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

            using (var cliLog = new StringLogger(template, log))
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
                };

                foreach (var target in targets.OrderBy(x => x.Name))
                {
                    await target.ImportAsync(syncService, syncOptions, session);
                }
            }
        }

        private static async Task<ISyncService> CreateSyncServiceAsync(string repository, ISession session)
        {
            var fs = await FileSystems.CreateAsync(repository, session.WorkingDirectory);

            return new SyncService(fs, session);
        }

        private ISession CreateSession(IAppEntity app)
        {
            var client = app.Clients.First();

            return new Session(
                app.Name,
                new DirectoryInfo(Path.GetTempPath()),
                new SquidexClientManager(new SquidexOptions
                {
                    Configurator = AcceptAllCertificatesConfigurator.Instance,
                    AppName = app.Name,
                    ClientId = $"{app.Name}:{client.Key}",
                    ClientSecret = client.Value.Secret,
                    Url = urlGenerator.Root()
                }));
        }
    }
}
