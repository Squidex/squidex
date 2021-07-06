// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using OpenIddict.Abstractions;

namespace Squidex.Domain.Users.InMemory
{
    public sealed class ImmutableApplication
    {
        public string Id { get; }

        public string? ClientId { get; }

        public string? ClientSecret { get; }

        public string? ConsentType { get; }

        public string? DisplayName { get; }

        public string? Type { get; }

        public ImmutableDictionary<CultureInfo, string> DisplayNames { get; }

        public ImmutableArray<string> Permissions { get; }

        public ImmutableArray<string> PostLogoutRedirectUris { get; }

        public ImmutableArray<string> RedirectUris { get; }

        public ImmutableArray<string> Requirements { get; }

        public ImmutableDictionary<string, JsonElement> Properties { get; }

        public ImmutableApplication(string id, OpenIddictApplicationDescriptor descriptor)
        {
            Id = id;
            ClientId = descriptor.ClientId;
            ClientSecret = descriptor.ClientSecret;
            ConsentType = descriptor.ConsentType;
            DisplayName = descriptor.DisplayName;
            DisplayNames = descriptor.DisplayNames.ToImmutableDictionary();
            Permissions = descriptor.Permissions.ToImmutableArray();
            PostLogoutRedirectUris = descriptor.PostLogoutRedirectUris.Select(x => x.ToString()).ToImmutableArray();
            Properties = descriptor.Properties.ToImmutableDictionary();
            RedirectUris = descriptor.RedirectUris.Select(x => x.ToString()).ToImmutableArray();
            Requirements = descriptor.Requirements.ToImmutableArray();
            Type = descriptor.Type;
        }
    }
}
