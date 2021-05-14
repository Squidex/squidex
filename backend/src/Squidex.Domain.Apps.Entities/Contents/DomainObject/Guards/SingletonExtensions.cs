// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject.Guards
{
    public static class SingletonExtensions
    {
        public static void MustNotCreateForUnpublishedSchema(this OperationContext context)
        {
            if (!context.SchemaDef.IsPublished && context.SchemaDef.Type != SchemaType.Singleton)
            {
                throw new DomainException(T.Get("contents.schemaNotPublished"));
            }
        }

        public static void MustNotCreateSingleton(this OperationContext context)
        {
            if (context.SchemaDef.Type == SchemaType.Singleton && context.ContentId != context.Schema.Id)
            {
                throw new DomainException(T.Get("contents.singletonNotCreatable"));
            }
        }

        public static void MustNotChangeSingleton(this OperationContext context, Status status)
        {
            if (context.SchemaDef.Type == SchemaType.Singleton && (context.Content.NewStatus == null || status != Status.Published))
            {
                throw new DomainException(T.Get("contents.singletonNotChangeable"));
            }
        }

        public static void MustNotDeleteSingleton(this OperationContext context)
        {
            if (context.SchemaDef.Type == SchemaType.Singleton)
            {
                throw new DomainException(T.Get("contents.singletonNotDeletable"));
            }
        }
    }
}
