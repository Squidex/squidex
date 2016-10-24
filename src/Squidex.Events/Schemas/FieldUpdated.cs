// ==========================================================================
//  FieldUpdated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Events.Schemas
{
    [TypeName("FieldUpdatedEvent")]
    public class FieldUpdated : IEvent
    {
        public long FieldId { get; set; }

        public FieldProperties Properties { get; set; }
    }
}
