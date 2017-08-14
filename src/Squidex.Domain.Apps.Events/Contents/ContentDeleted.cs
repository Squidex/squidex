// ==========================================================================
//  ContentDeleted.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Contents
{
    [TypeName("ContentDeletedEvent")]
    public sealed class ContentDeleted : ContentEvent
    {
    }
}
