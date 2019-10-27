// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Events.Schemas
{
    public sealed class SchemaCreatedField : SchemaCreatedFieldBase
    {
        public string Partitioning { get; set; }

        public List<SchemaCreatedNestedField> Nested { get; set; }
    }
}
