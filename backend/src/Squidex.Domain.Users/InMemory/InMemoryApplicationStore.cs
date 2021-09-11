// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OpenIddict.Abstractions;

namespace Squidex.Domain.Users.InMemory
{
    public class InMemoryApplicationStore : IOpenIddictApplicationStore<ImmutableApplication>
    {
        private readonly List<ImmutableApplication> applications;

        public InMemoryApplicationStore(params (string Id, OpenIddictApplicationDescriptor Descriptor)[] applications)
        {
            this.applications = applications.Select(x => new ImmutableApplication(x.Id, x.Descriptor)).ToList();
        }

        public InMemoryApplicationStore(IEnumerable<(string Id, OpenIddictApplicationDescriptor Descriptor)> applications)
        {
            this.applications = applications.Select(x => new ImmutableApplication(x.Id, x.Descriptor)).ToList();
        }

        public virtual ValueTask<long> CountAsync(
            CancellationToken cancellationToken)
        {
            return new ValueTask<long>(applications.Count);
        }

        public virtual ValueTask<long> CountAsync<TResult>(Func<IQueryable<ImmutableApplication>, IQueryable<TResult>> query,
            CancellationToken cancellationToken)
        {
            return query(applications.AsQueryable()).LongCount().AsValueTask();
        }

        public virtual ValueTask<TResult> GetAsync<TState, TResult>(Func<IQueryable<ImmutableApplication>, TState, IQueryable<TResult>> query, TState state,
            CancellationToken cancellationToken)
        {
            var result = query(applications.AsQueryable(), state).First();

            return result.AsValueTask();
        }

        public virtual ValueTask<ImmutableApplication?> FindByIdAsync(string identifier,
            CancellationToken cancellationToken)
        {
            var result = applications.Find(x => x.Id == identifier);

            return result.AsValueTask();
        }

        public virtual ValueTask<ImmutableApplication?> FindByClientIdAsync(string identifier,
            CancellationToken cancellationToken)
        {
            var result = applications.Find(x => x.ClientId == identifier);

            return result.AsValueTask();
        }

        public virtual async IAsyncEnumerable<ImmutableApplication> FindByPostLogoutRedirectUriAsync(string address,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var result = applications.Where(x => x.PostLogoutRedirectUris.Contains(address));

            foreach (var item in result)
            {
                yield return await Task.FromResult(item);
            }
        }

        public virtual async IAsyncEnumerable<ImmutableApplication> FindByRedirectUriAsync(string address,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var result = applications.Where(x => x.RedirectUris.Contains(address));

            foreach (var item in result)
            {
                yield return await Task.FromResult(item);
            }
        }

        public virtual async IAsyncEnumerable<ImmutableApplication> ListAsync(int? count, int? offset,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var result = applications;

            foreach (var item in result)
            {
                yield return await Task.FromResult(item);
            }
        }

        public virtual async IAsyncEnumerable<TResult> ListAsync<TState, TResult>(Func<IQueryable<ImmutableApplication>, TState, IQueryable<TResult>> query, TState state,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var result = query(applications.AsQueryable(), state);

            foreach (var item in result)
            {
                yield return await Task.FromResult(item);
            }
        }

        public virtual ValueTask<string?> GetIdAsync(ImmutableApplication application,
            CancellationToken cancellationToken)
        {
            return new ValueTask<string?>(application.Id);
        }

        public virtual ValueTask<string?> GetClientIdAsync(ImmutableApplication application,
            CancellationToken cancellationToken)
        {
            return application.ClientId.AsValueTask();
        }

        public virtual ValueTask<string?> GetClientSecretAsync(ImmutableApplication application,
            CancellationToken cancellationToken)
        {
            return application.ClientSecret.AsValueTask();
        }

        public virtual ValueTask<string?> GetClientTypeAsync(ImmutableApplication application,
            CancellationToken cancellationToken)
        {
            return application.Type.AsValueTask();
        }

        public virtual ValueTask<string?> GetConsentTypeAsync(ImmutableApplication application,
            CancellationToken cancellationToken)
        {
            return application.ConsentType.AsValueTask();
        }

        public virtual ValueTask<string?> GetDisplayNameAsync(ImmutableApplication application,
            CancellationToken cancellationToken)
        {
            return application.DisplayName.AsValueTask();
        }

        public virtual ValueTask<ImmutableDictionary<CultureInfo, string>> GetDisplayNamesAsync(ImmutableApplication application,
            CancellationToken cancellationToken)
        {
            return application.DisplayNames.AsValueTask();
        }

        public virtual ValueTask<ImmutableArray<string>> GetPermissionsAsync(ImmutableApplication application,
            CancellationToken cancellationToken)
        {
            return application.Permissions.AsValueTask();
        }

        public virtual ValueTask<ImmutableArray<string>> GetPostLogoutRedirectUrisAsync(ImmutableApplication application,
            CancellationToken cancellationToken)
        {
            return application.PostLogoutRedirectUris.AsValueTask();
        }

        public virtual ValueTask<ImmutableArray<string>> GetRedirectUrisAsync(ImmutableApplication application,
            CancellationToken cancellationToken)
        {
            return application.RedirectUris.AsValueTask();
        }

        public virtual ValueTask<ImmutableArray<string>> GetRequirementsAsync(ImmutableApplication application,
            CancellationToken cancellationToken)
        {
            return application.Requirements.AsValueTask();
        }

        public virtual ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(ImmutableApplication application,
            CancellationToken cancellationToken)
        {
            return application.Properties.AsValueTask();
        }

        public virtual ValueTask CreateAsync(ImmutableApplication application,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask UpdateAsync(ImmutableApplication application,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask DeleteAsync(ImmutableApplication application,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask<ImmutableApplication> InstantiateAsync(
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetClientIdAsync(ImmutableApplication application, string? identifier,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetClientSecretAsync(ImmutableApplication application, string? secret,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetClientTypeAsync(ImmutableApplication application, string? type,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetConsentTypeAsync(ImmutableApplication application, string? type,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetDisplayNameAsync(ImmutableApplication application, string? name,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetDisplayNamesAsync(ImmutableApplication application, ImmutableDictionary<CultureInfo, string> names,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetPermissionsAsync(ImmutableApplication application, ImmutableArray<string> permissions,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetPostLogoutRedirectUrisAsync(ImmutableApplication application, ImmutableArray<string> addresses,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetRedirectUrisAsync(ImmutableApplication application, ImmutableArray<string> addresses,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetPropertiesAsync(ImmutableApplication application, ImmutableDictionary<string, JsonElement> properties,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public virtual ValueTask SetRequirementsAsync(ImmutableApplication application, ImmutableArray<string> requirements,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }
    }
}
