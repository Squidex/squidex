// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Teams;

namespace Squidex.Domain.Apps.Events.Teams;

public sealed class TeamAuthChanged : TeamEvent
{
    public AuthScheme? Scheme { get; set; }
}
