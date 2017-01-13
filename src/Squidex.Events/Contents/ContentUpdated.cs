// ==========================================================================
//  ContentUpdated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Events.Contents
{
    [TypeName("ContentUpdatedEvent")]
    public class ContentUpdated : IEvent
    {
        public JObject Data { get; set; }
    }
}
