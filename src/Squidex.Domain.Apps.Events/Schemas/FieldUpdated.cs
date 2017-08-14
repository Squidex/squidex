// ==========================================================================
//  FieldUpdated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Schemas
{
    [TypeName("FieldUpdatedEvent")]
    public sealed class FieldUpdated : FieldEvent
    {
        public FieldProperties Properties { get; set; }
    }
}
