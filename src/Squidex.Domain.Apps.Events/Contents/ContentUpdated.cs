// ==========================================================================
//  ContentUpdated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Events.Contents
{
    [TypeName("ContentUpdatedEvent")]
    public class ContentUpdated : ContentEvent
    {
        public NamedContentData Data { get; set; }
    }
}
