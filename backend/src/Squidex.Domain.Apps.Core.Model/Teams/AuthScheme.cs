// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Teams;

public sealed record AuthScheme
{
    public string Domain { get; init; }

    public string DisplayName { get; init; }

    public string ClientId { get; init; }

    public string ClientSecret { get; init; }

    public string Authority { get; init; }

    public string? SignoutRedirectUrl { get; init; }
}
