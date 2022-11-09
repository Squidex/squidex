// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Immutable;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using OpenIddict.Abstractions;

namespace Squidex.Domain.Users.InMemory;

public class InMemoryScopeStore : IOpenIddictScopeStore<ImmutableScope>
{
    private readonly List<ImmutableScope> scopes;

    public InMemoryScopeStore(params (string Id, OpenIddictScopeDescriptor Descriptor)[] scopes)
    {
        this.scopes = scopes.Select(x => new ImmutableScope(x.Id, x.Descriptor)).ToList();
    }

    public InMemoryScopeStore(IEnumerable<(string Id, OpenIddictScopeDescriptor Descriptor)> scopes)
    {
        this.scopes = scopes.Select(x => new ImmutableScope(x.Id, x.Descriptor)).ToList();
    }

    public virtual ValueTask<long> CountAsync(
        CancellationToken cancellationToken)
    {
        return new ValueTask<long>(scopes.Count);
    }

    public virtual ValueTask<long> CountAsync<TResult>(Func<IQueryable<ImmutableScope>, IQueryable<TResult>> query,
        CancellationToken cancellationToken)
    {
        return query(scopes.AsQueryable()).LongCount().AsValueTask();
    }

    public virtual ValueTask<TResult> GetAsync<TState, TResult>(Func<IQueryable<ImmutableScope>, TState, IQueryable<TResult>> query, TState state,
        CancellationToken cancellationToken)
    {
        var result = query(scopes.AsQueryable(), state).First();

        return result.AsValueTask();
    }

    public virtual ValueTask<ImmutableScope?> FindByIdAsync(string identifier,
        CancellationToken cancellationToken)
    {
        var result = scopes.Find(x => x.Id == identifier);

        return result.AsValueTask();
    }

    public virtual ValueTask<ImmutableScope?> FindByNameAsync(string name,
        CancellationToken cancellationToken)
    {
        var result = scopes.Find(x => x.Name == name);

        return result.AsValueTask();
    }

    public virtual async IAsyncEnumerable<ImmutableScope> FindByNamesAsync(ImmutableArray<string> names,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var result = scopes.Where(x => x.Name != null && names.Contains(x.Name));

        foreach (var item in result)
        {
            yield return await Task.FromResult(item);
        }
    }

    public virtual async IAsyncEnumerable<ImmutableScope> FindByResourceAsync(string resource,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var result = scopes.Where(x => x.Resources.Contains(resource));

        foreach (var item in result)
        {
            yield return await Task.FromResult(item);
        }
    }

    public virtual async IAsyncEnumerable<ImmutableScope> ListAsync(int? count, int? offset,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var result = scopes;

        foreach (var item in result)
        {
            yield return await Task.FromResult(item);
        }
    }

    public virtual async IAsyncEnumerable<TResult> ListAsync<TState, TResult>(Func<IQueryable<ImmutableScope>, TState, IQueryable<TResult>> query, TState state,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var result = query(scopes.AsQueryable(), state);

        foreach (var item in result)
        {
            yield return await Task.FromResult(item);
        }
    }

    public virtual ValueTask<string?> GetIdAsync(ImmutableScope scope,
        CancellationToken cancellationToken)
    {
        return new ValueTask<string?>(scope.Id);
    }

    public virtual ValueTask<string?> GetNameAsync(ImmutableScope scope,
        CancellationToken cancellationToken)
    {
        return scope.Name.AsValueTask();
    }

    public virtual ValueTask<string?> GetDescriptionAsync(ImmutableScope scope,
        CancellationToken cancellationToken)
    {
        return scope.Description.AsValueTask();
    }

    public virtual ValueTask<string?> GetDisplayNameAsync(ImmutableScope scope,
        CancellationToken cancellationToken)
    {
        return scope.DisplayName.AsValueTask();
    }

    public virtual ValueTask<ImmutableDictionary<CultureInfo, string>> GetDescriptionsAsync(ImmutableScope scope,
        CancellationToken cancellationToken)
    {
        return scope.Descriptions.AsValueTask();
    }

    public virtual ValueTask<ImmutableDictionary<CultureInfo, string>> GetDisplayNamesAsync(ImmutableScope scope,
        CancellationToken cancellationToken)
    {
        return scope.DisplayNames.AsValueTask();
    }

    public virtual ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(ImmutableScope scope,
        CancellationToken cancellationToken)
    {
        return scope.Properties.AsValueTask();
    }

    public virtual ValueTask<ImmutableArray<string>> GetResourcesAsync(ImmutableScope scope,
        CancellationToken cancellationToken)
    {
        return scope.Resources.AsValueTask();
    }

    public virtual ValueTask CreateAsync(ImmutableScope scope,
        CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public virtual ValueTask UpdateAsync(ImmutableScope scope,
        CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public virtual ValueTask DeleteAsync(ImmutableScope scope,
        CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public virtual ValueTask<ImmutableScope> InstantiateAsync(
        CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public virtual ValueTask SetDescriptionAsync(ImmutableScope scope, string? description,
        CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public virtual ValueTask SetDescriptionsAsync(ImmutableScope scope, ImmutableDictionary<CultureInfo, string> descriptions,
        CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public virtual ValueTask SetDisplayNameAsync(ImmutableScope scope, string? name,
        CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public virtual ValueTask SetDisplayNamesAsync(ImmutableScope scope, ImmutableDictionary<CultureInfo, string> names,
        CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public virtual ValueTask SetNameAsync(ImmutableScope scope, string? name,
        CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public virtual ValueTask SetPropertiesAsync(ImmutableScope scope, ImmutableDictionary<string, JsonElement> properties,
        CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }

    public virtual ValueTask SetResourcesAsync(ImmutableScope scope, ImmutableArray<string> resources,
        CancellationToken cancellationToken)
    {
        throw new NotSupportedException();
    }
}
