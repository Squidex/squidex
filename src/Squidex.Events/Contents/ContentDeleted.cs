// ==========================================================================
//  ContentDeleted.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Events.Contents
{
    [TypeName("ContentDeletedEvent")]
    public class ContentDeleted : IEvent
    {
    }
}
