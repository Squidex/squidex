// ==========================================================================
//  FieldShown.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Events.Schemas
{
    [TypeName("FieldShownEvent")]
    public class FieldShown : IEvent
    {
        public long FieldId { get; set; }
    }
}
