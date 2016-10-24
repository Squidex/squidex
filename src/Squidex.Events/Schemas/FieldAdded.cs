// ==========================================================================
//  FieldAdded.cs
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
    [TypeName("FieldAddedEvent")]
    public class FieldAdded : IEvent
    {
        public long FieldId { get; set; }

        public string Name { get; set; }

        public FieldProperties Properties { get; set; }
    }
}
