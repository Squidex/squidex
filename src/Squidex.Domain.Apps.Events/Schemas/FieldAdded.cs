// ==========================================================================
//  FieldAdded.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Schemas
{
    [TypeName("FieldAddedEvent")]
    public sealed class FieldAdded : FieldEvent
    {
        public string Name { get; set; }

        public string Partitioning { get; set; }

        public FieldProperties Properties { get; set; }
    }
}
