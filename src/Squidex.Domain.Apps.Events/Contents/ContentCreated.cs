// ==========================================================================
//  ContentCreated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Contents
{
    [TypeName("ContentCreatedEvent")]
    public class ContentCreated : ContentEvent
    {
        public NamedContentData Data { get; set; }
    }
}
