// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Text.Extraction;

// The context is created for all contents of a schema in a batch and only used by a single thread.
public sealed class TextExtractionContext : IDisposable
{
    private readonly Dictionary<object, object?> items = [];

    required public NamedId<DomainId> AppId { get; init; }

    required public NamedId<DomainId> SchemaId { get; init; }

    public Schema? Schema { get; init; }

    public ResolvedComponents Components { get; init; } = ResolvedComponents.Empty;

    public void Dispose()
    {
        foreach (var item in items.Values.OfType<IDisposable>())
        {
            item.Dispose();
        }

        items.Clear();
    }

    public T GetOrAdd<T, TArg>(object key, TArg arg, Func<TArg, T> factory)
    {
        // The strategies can store expensive values for the schema, e.g. compiled scripts.
        if (items.TryGetValue(key, out var existing))
        {
            return (T)existing!;
        }

        // The argument allows static factories without closures, because this method is called for every content.
        var value = factory(arg);

        items[key] = value;
        return value;
    }

    public void Remove(object key)
    {
        if (items.Remove(key, out var existing) && existing is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
