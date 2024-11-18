// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Immutable;
using System.Globalization;
using System.Text.Json;
using OpenIddict.Abstractions;

namespace Squidex.Domain.Users.InMemory;

public sealed class ImmutableScope(string id, OpenIddictScopeDescriptor descriptor)
{
    public string Id { get; } = id;

    public string? Name { get; } = descriptor.Name;

    public string? Description { get; } = descriptor.Description;

    public string? DisplayName { get; } = descriptor.DisplayName;

    public ImmutableDictionary<CultureInfo, string> Descriptions { get; } = descriptor.Descriptions.ToImmutableDictionary();

    public ImmutableDictionary<CultureInfo, string> DisplayNames { get; } = descriptor.DisplayNames.ToImmutableDictionary();

    public ImmutableDictionary<string, JsonElement> Properties { get; } = descriptor.Properties.ToImmutableDictionary();

    public ImmutableArray<string> Resources { get; } = descriptor.Resources.ToImmutableArray();
}
