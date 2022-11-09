// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Apps.Commands;

public sealed class UpdateClient : AppCommand
{
    public string Id { get; set; }

    public string? Name { get; set; }

    public string? Role { get; set; }

    public long? ApiCallsLimit { get; set; }

    public long? ApiTrafficLimit { get; set; }

    public bool? AllowAnonymous { get; set; }
}
