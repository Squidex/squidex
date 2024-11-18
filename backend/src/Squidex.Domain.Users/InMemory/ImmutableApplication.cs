// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Immutable;
using System.Globalization;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;

namespace Squidex.Domain.Users.InMemory;

public sealed class ImmutableApplication(string id, OpenIddictApplicationDescriptor descriptor)
{
    public string Id { get; } = id;

    public string? ClientId { get; } = descriptor.ClientId;

    public string? ClientSecret { get; } = descriptor.ClientSecret;

    public string? ClientType { get; } = descriptor.ClientType;

    public string? ConsentType { get; } = descriptor.ConsentType;

    public string? DisplayName { get; } = descriptor.DisplayName;

    public string? ApplicationType { get; } = descriptor.ApplicationType;

    public JsonWebKeySet? JsonWebKeySet { get; } = descriptor.JsonWebKeySet;

    public ImmutableDictionary<CultureInfo, string> DisplayNames { get; } = descriptor.DisplayNames.ToImmutableDictionary();

    public ImmutableArray<string> Permissions { get; } = descriptor.Permissions.ToImmutableArray();

    public ImmutableArray<string> PostLogoutRedirectUris { get; } = descriptor.PostLogoutRedirectUris.Select(x => x.ToString()).ToImmutableArray();

    public ImmutableArray<string> RedirectUris { get; } = descriptor.RedirectUris.Select(x => x.ToString()).ToImmutableArray();

    public ImmutableArray<string> Requirements { get; } = descriptor.Requirements.ToImmutableArray();

    public ImmutableDictionary<string, JsonElement> Properties { get; } = descriptor.Properties.ToImmutableDictionary();

    public ImmutableDictionary<string, string> Settings { get; } = descriptor.Settings.ToImmutableDictionary();
}
