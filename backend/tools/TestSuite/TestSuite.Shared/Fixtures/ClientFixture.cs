// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using Squidex.ClientLibrary.Management;
using Xunit;

namespace TestSuite.Fixtures;

public class ClientFixture : IAsyncLifetime
{
    public ClientManagerWrapper Squidex { get; private set; }

    public string AppName => ClientManager.Options.AppName;

    public string ClientId => ClientManager.Options.ClientId;

    public string ClientSecret => ClientManager.Options.ClientSecret;

    public string Url => ClientManager.Options.Url;

    public ISquidexClientManager ClientManager => Squidex.ClientManager;

    public IAppsClient Apps
    {
        get => ClientManager.CreateAppsClient();
    }

    public IAssetsClient Assets
    {
        get => ClientManager.CreateAssetsClient();
    }

    public IBackupsClient Backups
    {
        get => ClientManager.CreateBackupsClient();
    }

    public ICommentsClient Comments
    {
        get => ClientManager.CreateCommentsClient();
    }

    public IDiagnosticsClient Diagnostics
    {
        get => ClientManager.CreateDiagnosticsClient();
    }

    public IHistoryClient History
    {
        get => ClientManager.CreateHistoryClient();
    }

    public ILanguagesClient Languages
    {
        get => ClientManager.CreateLanguagesClient();
    }

    public IPingClient Ping
    {
        get => ClientManager.CreatePingClient();
    }

    public IPlansClient Plans
    {
        get => ClientManager.CreatePlansClient();
    }

    public IRulesClient Rules
    {
        get => ClientManager.CreateRulesClient();
    }

    public ISchemasClient Schemas
    {
        get => ClientManager.CreateSchemasClient();
    }

    public ISearchClient Search
    {
        get => ClientManager.CreateSearchClient();
    }

    public ITemplatesClient Templates
    {
        get => ClientManager.CreateTemplatesClient();
    }

    public ITranslationsClient Translations
    {
        get => ClientManager.CreateTranslationsClient();
    }

    public IUserManagementClient UserManagement
    {
        get => ClientManager.CreateUserManagementClient();
    }

    public IContentsSharedClient<DynamicContent, DynamicData> SharedContents
    {
        get => ClientManager.CreateSharedDynamicContentsClient();
    }

    static ClientFixture()
    {
        VerifierSettings.IgnoreMember("AppName");
        VerifierSettings.IgnoreMember("Created");
        VerifierSettings.IgnoreMember("CreatedBy");
        VerifierSettings.IgnoreMember("EditToken");
        VerifierSettings.IgnoreMember("Href");
        VerifierSettings.IgnoreMember("LastModified");
        VerifierSettings.IgnoreMember("LastModifiedBy");
        VerifierSettings.IgnoreMember("RoleProperties");
        VerifierSettings.IgnoreMember("SchemaName");
        VerifierSettings.IgnoreMembersWithType<DateTimeOffset>();
    }

    public virtual async Task InitializeAsync()
    {
        Squidex = await Factories.CreateAsync(nameof(ClientManagerWrapper), async () =>
        {
            var clientManager = new ClientManagerWrapper();

            await clientManager.ConnectAsync();

            return clientManager;
        });
    }

    public virtual Task DisposeAsync()
    {
        return Task.CompletedTask;
    }
}
