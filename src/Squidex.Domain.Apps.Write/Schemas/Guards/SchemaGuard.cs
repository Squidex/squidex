// ==========================================================================
//  SchemaGuard.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Schemas.Guards
{
    public static class SchemaGuard
    {
        public static void GuardCanReorder(Schema schema, List<long> fieldIds)
        {
            if (fieldIds.Count != schema.Fields.Count || fieldIds.Any(x => !schema.FieldsById.ContainsKey(x)))
            {
                var error = new ValidationError("Ids must cover all fields.", "FieldIds");

                throw new ValidationException("Cannot reorder schema fields.", error);
            }
        }

        public static void GuardCanPublish(Schema schema)
        {
            if (schema.IsPublished)
            {
                throw new DomainException("Schema is already published.");
            }
        }

        public static void GuardCanUnpublish(Schema schema)
        {
            if (!schema.IsPublished)
            {
                throw new DomainException("Schema is not published.");
            }
        }
    }
}
