// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Configuration;
using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Configuration;
using Squidex.ClientLibrary.Management;
using TestSuite.Utils;

namespace TestSuite
{
    public sealed class ClientManagerWrapper
    {
        private readonly Lazy<IAppsClient> apps;
        private readonly Lazy<IAssetsClient> assets;
        private readonly Lazy<ICommentsClient> comments;
        private readonly Lazy<IBackupsClient> backups;
        private readonly Lazy<IDiagnosticsClient> diagnostics;
        private readonly Lazy<IHistoryClient> history;
        private readonly Lazy<ILanguagesClient> languages;
        private readonly Lazy<IPingClient> ping;
        private readonly Lazy<IPlansClient> plans;
        private readonly Lazy<IRulesClient> rules;
        private readonly Lazy<ISchemasClient> schemas;
        private readonly Lazy<ISearchClient> search;
        private readonly Lazy<ITemplatesClient> templates;
        private readonly Lazy<ITranslationsClient> translations;

        public SquidexClientManager ClientManager { get; }

        public IAppsClient Apps
        {
            get => apps.Value;
        }

        public IAssetsClient Assets
        {
            get => assets.Value;
        }

        public IBackupsClient Backups
        {
            get => backups.Value;
        }

        public ICommentsClient Comments
        {
            get => comments.Value;
        }

        public IDiagnosticsClient Diagnostics
        {
            get => diagnostics.Value;
        }

        public IHistoryClient History
        {
            get => history.Value;
        }

        public ILanguagesClient Languages
        {
            get => languages.Value;
        }

        public IPingClient Ping
        {
            get => ping.Value;
        }

        public IPlansClient Plans
        {
            get => plans.Value;
        }

        public IRulesClient Rules
        {
            get => rules.Value;
        }

        public ISchemasClient Schemas
        {
            get => schemas.Value;
        }

        public ISearchClient Search
        {
            get => search.Value;
        }

        public ITemplatesClient Templates
        {
            get => templates.Value;
        }

        public ITranslationsClient Translations
        {
            get => translations.Value;
        }

        public ClientManagerWrapper()
        {
            var appName = TestHelpers.GetAndPrintValue("config:app:name", "integration-tests");
            var clientId = TestHelpers.GetAndPrintValue("config:client:id", "root");
            var clientSecret = TestHelpers.GetAndPrintValue("config:client:secret", "xeLd6jFxqbXJrfmNLlO2j1apagGGGSyZJhFnIuHp4I0=");
            var serverUrl = TestHelpers.GetAndPrintValue("config:server:url", "https://localhost:5001");

            ClientManager = new SquidexClientManager(new SquidexOptions
            {
                AppName = appName,
                ClientId = clientId,
                ClientSecret = clientSecret,
                ClientFactory = null,
                Configurator = AcceptAllCertificatesConfigurator.Instance,
                ReadResponseAsString = true,
                Url = serverUrl
            });

            apps = new Lazy<IAppsClient>(() =>
            {
                return ClientManager.CreateAppsClient();
            });

            assets = new Lazy<IAssetsClient>(() =>
            {
                return ClientManager.CreateAssetsClient();
            });

            backups = new Lazy<IBackupsClient>(() =>
            {
                return ClientManager.CreateBackupsClient();
            });

            comments = new Lazy<ICommentsClient>(() =>
            {
                return ClientManager.CreateCommentsClient();
            });

            diagnostics = new Lazy<IDiagnosticsClient>(() =>
            {
                return ClientManager.CreateDiagnosticsClient();
            });

            history = new Lazy<IHistoryClient>(() =>
            {
                return ClientManager.CreateHistoryClient();
            });

            languages = new Lazy<ILanguagesClient>(() =>
            {
                return ClientManager.CreateLanguagesClient();
            });

            ping = new Lazy<IPingClient>(() =>
            {
                return ClientManager.CreatePingClient();
            });

            plans = new Lazy<IPlansClient>(() =>
            {
                return ClientManager.CreatePlansClient();
            });

            rules = new Lazy<IRulesClient>(() =>
            {
                return ClientManager.CreateRulesClient();
            });

            schemas = new Lazy<ISchemasClient>(() =>
            {
                return ClientManager.CreateSchemasClient();
            });

            search = new Lazy<ISearchClient>(() =>
            {
                return ClientManager.CreateSearchClient();
            });

            templates = new Lazy<ITemplatesClient>(() =>
            {
                return ClientManager.CreateTemplatesClient();
            });

            translations = new Lazy<ITranslationsClient>(() =>
            {
                return ClientManager.CreateTranslationsClient();
            });
        }

        public async Task<ClientManagerWrapper> ConnectAsync()
        {
            var waitSeconds = TestHelpers.Configuration.GetValue<int>("config:wait");

            if (waitSeconds > 10)
            {
                Console.WriteLine("Waiting {0} seconds to access server", waitSeconds);

                var pingClient = ClientManager.CreatePingClient();
                try
                {
                    using (var cts = new CancellationTokenSource(waitSeconds * 1000))
                    {
                        while (!cts.IsCancellationRequested)
                        {
                            try
                            {
                                await pingClient.GetPingAsync(cts.Token);
                                break;
                            }
                            catch
                            {
                                await Task.Delay(100, cts.Token);
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    throw new InvalidOperationException("Cannot connect to test system.");
                }

                Console.WriteLine("Connected to server.");
            }
            else
            {
                Console.WriteLine("Waiting for server is skipped.");
            }

            return this;
        }
    }
}
