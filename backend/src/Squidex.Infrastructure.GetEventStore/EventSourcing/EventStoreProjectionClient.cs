// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using EventStore.Client;
using Squidex.Text;

namespace Squidex.Infrastructure.EventSourcing;

public sealed class EventStoreProjectionClient
{
    private readonly ConcurrentDictionary<string, bool> projections = new ConcurrentDictionary<string, bool>();
    private readonly string projectionPrefix;
    private readonly EventStoreProjectionManagementClient client;

    public EventStoreProjectionClient(EventStoreClientSettings settings, string projectionPrefix)
    {
        client = new EventStoreProjectionManagementClient(settings);

        this.projectionPrefix = projectionPrefix;
    }

    private string CreateFilterProjectionName(string filter)
    {
        return $"by-{projectionPrefix.Slugify()}-{filter.Slugify()}";
    }

    public async Task<string> CreateProjectionAsync(StreamFilter filter)
    {
        if (filter.Kind == StreamFilterKind.MatchFull && filter.Prefixes?.Count == 1)
        {
            return $"{projectionPrefix}-{filter.Prefixes[0]}";
        }

        var regex = filter.ToRegex();

        var name = CreateFilterProjectionName(regex);

        var query =
            $@"fromAll()
                    .when({{
                        $any: function (s, e) {{
                            if (e.streamId.indexOf('{projectionPrefix}') === 0 && /{regex}/.test(e.streamId.substring({projectionPrefix.Length + 1}))) {{
                                linkTo('{name}', e);
                            }}
                        }}
                    }});";

        await CreateProjectionAsync(name, query);

        return name;
    }

    private async Task CreateProjectionAsync(string name, string query)
    {
        if (projections.TryAdd(name, true))
        {
            try
            {
                await client.CreateContinuousAsync(name, "fromAll().when()");
                await client.UpdateAsync(name, query, true);
            }
            catch (Exception ex)
            {
                if (!ex.Is<InvalidOperationException>())
                {
                    throw;
                }
            }
        }
    }
}
