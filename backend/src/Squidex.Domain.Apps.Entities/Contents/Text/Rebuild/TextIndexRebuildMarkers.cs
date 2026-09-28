// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.States;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Entities.Contents.Text.Rebuild;

// Marks the rebuilds that have not been handed back yet, e.g. because the worker has crashed.
public sealed class TextIndexRebuildMarkers(IPersistenceFactory<TextIndexRebuildMarkers.State> persistenceFactory) : IDeleter
{
    private static readonly DomainId Key = DomainId.Create("Default");

    public sealed class State
    {
        public List<TextIndexRebuildMarker> Markers { get; set; } = [];
    }

    public async Task<List<TextIndexRebuildMarker>> GetAsync(
        CancellationToken ct = default)
    {
        var state = GetState();

        await state.LoadAsync(ct);

        return state.Value.Markers;
    }

    public Task AddAsync(DomainId appId, DomainId? schemaId,
        CancellationToken ct = default)
    {
        return GetState().UpdateAsync(state =>
        {
            state.Markers.RemoveAll(x => x.AppId == appId);
            state.Markers.Add(new TextIndexRebuildMarker { AppId = appId, SchemaId = schemaId });
            return true;
        }, ct: ct);
    }

    public Task RemoveAsync(DomainId appId,
        CancellationToken ct = default)
    {
        return GetState().UpdateAsync(state => state.Markers.RemoveAll(x => x.AppId == appId) > 0, ct: ct);
    }

    Task IDeleter.DeleteAppAsync(App app,
        CancellationToken ct)
    {
        return RemoveAsync(app.Id, ct);
    }

    Task IDeleter.DeleteSchemaAsync(App app, Schema schema,
        CancellationToken ct)
    {
        return Task.CompletedTask;
    }

    private SimpleState<State> GetState()
    {
        return new SimpleState<State>(persistenceFactory, typeof(TextIndexRebuildMarkers), Key);
    }
}

public sealed class TextIndexRebuildMarker
{
    public DomainId AppId { get; set; }

    public DomainId? SchemaId { get; set; }
}
