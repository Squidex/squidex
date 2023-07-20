// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Scripting;

public sealed class EventScriptVars : ScriptVars
{
    public DomainId AppId
    {
        set => SetInitial(value);
    }

    public string AppName
    {
        set => SetInitial(value);
    }

    public ClaimsPrincipal User
    {
        set => SetInitial(value);
    }

    public EnrichedEvent Event
    {
        set => SetInitial(value);
    }
}
