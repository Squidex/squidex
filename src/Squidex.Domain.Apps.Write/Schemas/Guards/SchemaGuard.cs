// ==========================================================================
//  SchemaGuard.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Schemas.Guards
{
    public static class SchemaGuard
    {
        public static void GuardValidSchemaName(string name)
        {
            if (!name.IsSlug())
            {
                var error = new ValidationError("Name must be a valid slug", "Name");

                throw new ValidationException("Cannot create a new schema", error);
            }
        }

        public static void GuardCanPublish(Schema schema)
        {
            if (schema.IsPublished)
            {
                throw new DomainException("Schema is already published");
            }
        }

        public static void GuardCanUnpublish(Schema schema)
        {
            if (!schema.IsPublished)
            {
                throw new DomainException("Schema is not published");
            }
        }
    }
}
