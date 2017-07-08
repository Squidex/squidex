// ==========================================================================
//  SchemaCreatedField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Core.Schemas;

namespace Squidex.Events.Schemas
{
    public sealed class SchemaCreatedField
    {
        public string Partitioning { get; set; }

        public string Name { get; set; }

        public bool IsHidden { get; set; }

        public bool IsDisabled { get; set; }

        public FieldProperties Properties { get; set; }
    }
}
