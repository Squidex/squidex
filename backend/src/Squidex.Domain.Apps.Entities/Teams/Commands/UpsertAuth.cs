// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Teams;

namespace Squidex.Domain.Apps.Entities.Teams.Commands;

public sealed class UpsertAuth : TeamCommand
{
    public AuthScheme? Scheme { get; set; }
}
