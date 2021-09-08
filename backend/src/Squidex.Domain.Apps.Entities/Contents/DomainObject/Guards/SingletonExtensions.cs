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
        public static void MustNotCreateForUnpublishedSchema(this ContentOperation operation)
        {
            if (!operation.SchemaDef.IsPublished && operation.SchemaDef.Type != SchemaType.Singleton)
            {
                throw new DomainException(T.Get("contents.schemaNotPublished"));
            }
        }

        public static void MustNotCreateSingleton(this ContentOperation operation)
        {
            if (operation.SchemaDef.Type == SchemaType.Singleton && operation.CommandId != operation.Schema.Id)
            {
                throw new DomainException(T.Get("contents.singletonNotCreatable"));
            }
        }

        public static void MustNotChangeSingleton(this ContentOperation operation, Status status)
        {
            if (operation.SchemaDef.Type == SchemaType.Singleton && (operation.Snapshot.NewStatus == null || status != Status.Published))
            {
                throw new DomainException(T.Get("contents.singletonNotChangeable"));
            }
        }

        public static void MustNotDeleteSingleton(this ContentOperation operation)
        {
            if (operation.SchemaDef.Type == SchemaType.Singleton)
            {
                throw new DomainException(T.Get("contents.singletonNotDeletable"));
            }
        }
    }
}
