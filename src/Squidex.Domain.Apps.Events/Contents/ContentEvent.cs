// ==========================================================================
//  ContentEvent.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Events.Contents
{
    public abstract class ContentEvent : SchemaEvent
    {
        public Guid ContentId { get; set; }
    }
}
