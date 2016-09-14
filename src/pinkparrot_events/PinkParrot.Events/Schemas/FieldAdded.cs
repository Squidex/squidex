// ==========================================================================
//  FieldAdded.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using PinkParrot.Core.Schemas;
using PinkParrot.Infrastructure;

namespace PinkParrot.Events.Schemas
{
    [TypeName("FieldAddedEvent")]
    public class FieldAdded : TenantEvent
    {
        public long FieldId { get; set; }

        public string Name { get; set; }

        public IFieldProperties Properties { get; set; }
    }
}
