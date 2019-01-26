// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using P = Squidex.Domain.Apps.Core.Partitioning;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands
{
    public sealed class UpsertSchemaField : UpsertSchemaFieldBase
    {
        public string Partitioning { get; set; } = P.Invariant.Key;

        public List<UpsertSchemaNestedField> Nested { get; set; }
    }
}
