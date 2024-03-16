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

namespace Squidex.Domain.Users.InMemory
{
    public sealed class ImmutableScope
    {
        public string Id { get; }

        public string? Name { get; }

        public string? Description { get; }

        public string? DisplayName { get; }

        public ImmutableDictionary<CultureInfo, string> Descriptions { get; }

        public ImmutableDictionary<CultureInfo, string> DisplayNames { get; }

        public ImmutableDictionary<string, JsonElement> Properties { get; }

        public ImmutableArray<string> Resources { get; }

        public ImmutableScope(string id, OpenIddictScopeDescriptor descriptor)
        {
            Id = id;
            Description = descriptor.Description;
            Descriptions = descriptor.Descriptions.ToImmutableDictionary();
            Name = descriptor.Name;
            DisplayName = descriptor.DisplayName;
            DisplayNames = descriptor.DisplayNames.ToImmutableDictionary();
            Properties = descriptor.Properties.ToImmutableDictionary();
            Resources = descriptor.Resources.ToImmutableArray();
        }
    }
}
