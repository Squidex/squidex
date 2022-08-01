// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.ClientLibrary;
using Xunit;

namespace TestSuite.Fixtures
{
    public class ClientManagerFixture : IAsyncLifetime
    {
        public ClientManagerWrapper Squidex { get; private set; }

        public string AppName => ClientManager.Options.AppName;

        public string ClientId => ClientManager.Options.ClientId;

        public string ClientSecret => ClientManager.Options.ClientSecret;

        public string ServerUrl => ClientManager.Options.Url;

        public SquidexClientManager ClientManager => Squidex.ClientManager;

        static ClientManagerFixture()
        {
            VerifierSettings.IgnoreMember("AppName");
            VerifierSettings.IgnoreMember("Created");
            VerifierSettings.IgnoreMember("CreatedBy");
            VerifierSettings.IgnoreMember("EditToken");
            VerifierSettings.IgnoreMember("Href");
            VerifierSettings.IgnoreMember("LastModified");
            VerifierSettings.IgnoreMember("LastModifiedBy");
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
}
