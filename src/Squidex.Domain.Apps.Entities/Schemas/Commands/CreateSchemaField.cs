// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands
{
    public sealed class CreateSchemaField : CreateSchemaFieldBase
    {
        public string Partitioning { get; set; } = "invariant";

        public List<CreateSchemaNestedField> Nested { get; set; }
    }
}
