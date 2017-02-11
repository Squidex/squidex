// ==========================================================================
//  ContentCreated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Events.Contents
{
    [TypeName("ContentCreatedEvent")]
    public class ContentCreated : ContentEvent
    {
        public ContentData Data { get; set; }
    }
}
