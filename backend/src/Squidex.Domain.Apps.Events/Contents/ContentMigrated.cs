// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Events.Contents;

[EventType(nameof(ContentMigrated))]
public sealed class ContentMigrated : ContentEvent
{
    public ContentData? Data { get; set; }

    public ContentData? NewData { get; set; }
}
