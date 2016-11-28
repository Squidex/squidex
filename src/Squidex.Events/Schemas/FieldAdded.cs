// ==========================================================================
//  FieldAdded.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Events.Schemas
{
    [TypeName("FieldAddedEvent")]
    public class FieldAdded : FieldEvent
    {
        public string Name { get; set; }

        public FieldProperties Properties { get; set; }
    }
}
