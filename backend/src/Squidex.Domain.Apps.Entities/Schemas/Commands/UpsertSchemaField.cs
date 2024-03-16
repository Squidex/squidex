// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Schemas.Commands
{
    public sealed class UpsertSchemaField : UpsertSchemaFieldBase
    {
        public string Partitioning { get; set; }

        public UpsertSchemaNestedField[]? Nested { get; set; }
    }
}
