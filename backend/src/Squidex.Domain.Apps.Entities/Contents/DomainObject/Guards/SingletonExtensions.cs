// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject.Test
{
    public static class SingletonExtensions
    {
        public static void MustNotCreateSingleton(this OperationContext context)
        {
            if (context.SchemaDef.IsSingleton && context.ContentId != context.Schema.Id)
            {
                throw new DomainException(T.Get("contents.singletonNotCreatable"));
            }
        }

        public static void MustNotChangeSingleton(this OperationContext context, Status status)
        {
            if (context.SchemaDef.IsSingleton && (context.Content.NewStatus == null || status != Status.Published))
            {
                throw new DomainException(T.Get("contents.singletonNotChangeable"));
            }
        }

        public static void MustNotDeleteSingleton(this OperationContext context)
        {
            if (context.SchemaDef.IsSingleton)
            {
                throw new DomainException(T.Get("contents.singletonNotDeletable"));
            }
        }
    }
}
