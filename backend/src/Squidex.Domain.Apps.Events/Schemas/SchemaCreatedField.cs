// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Events.Schemas
{
    public sealed class SchemaCreatedField : SchemaCreatedFieldBase
    {
        public string Partitioning { get; set; }

        public SchemaCreatedNestedField[] Nested { get; set; }
    }
}
