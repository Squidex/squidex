// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Apps;

public sealed record AppClient(string Name, string Secret)
{
    public string Name { get; init; } = Guard.NotNullOrEmpty(Name);

    public string Secret { get; } = Guard.NotNullOrEmpty(Secret);

    public string Role { get; init; } = "Editor";

    public long ApiCallsLimit { get; init; }

    public long ApiTrafficLimit { get; init; }

    public bool AllowAnonymous { get; init; }
}
