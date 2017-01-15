// ==========================================================================
//  ContentUpdated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Events.Contents
{
    [TypeName("ContentUpdatedEvent")]
    public class ContentUpdated : IEvent
    {
        public ContentData Data { get; set; }
    }
}
