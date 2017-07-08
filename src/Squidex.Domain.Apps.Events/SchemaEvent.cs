// ==========================================================================
//  SchemaEvent.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events
{
    public abstract class SchemaEvent : AppEvent
    {
        public NamedId<Guid> SchemaId { get; set; }
    }
}
