// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Contents;

[EventType(nameof(ContentStatusChanged), 2)]
public sealed class ContentStatusChanged : ContentEvent
{
    public StatusChange Change { get; set; }

    public Status Status { get; set; }

    public bool NewVersion { get; set; }
}
