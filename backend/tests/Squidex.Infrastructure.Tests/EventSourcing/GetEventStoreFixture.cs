// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using EventStore.Client;
using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Infrastructure.EventSourcing;

public sealed class GetEventStoreFixture : IAsyncLifetime
{
    private readonly EventStoreClientSettings settings;

    public GetEventStore EventStore { get; }

    public GetEventStoreFixture()
    {
        AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

        settings = EventStoreClientSettings.Create(TestConfig.Configuration["eventStore:configuration"]);

        EventStore = new GetEventStore(settings, TestUtils.DefaultSerializer);
    }

    public Task InitializeAsync()
    {
        return EventStore.InitializeAsync(default);
    }

    public async Task DisposeAsync()
    {
        var projectionsManager = new EventStoreProjectionManagementClient(settings);

        await foreach (var projection in projectionsManager.ListAllAsync())
        {
            var name = projection.Name;

            if (name.StartsWith("by-squidex-test", StringComparison.OrdinalIgnoreCase))
            {
                await projectionsManager.DisableAsync(name);
            }
        }
    }
}
