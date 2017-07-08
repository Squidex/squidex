// ==========================================================================
//  FieldUpdated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Events.Schemas
{
    [TypeName("FieldUpdatedEvent")]
    public class FieldUpdated : FieldEvent
    {
        public FieldProperties Properties { get; set; }
    }
}
