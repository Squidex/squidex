// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL.Resolvers;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public static class EntityResolvers
    {
        public static readonly IFieldResolver Id = Resolve<IEntity>(x => x.Id.ToString());
        public static readonly IFieldResolver Created = Resolve<IEntity>(x => x.Created);
        public static readonly IFieldResolver CreatedBy = Resolve<IEntityWithCreatedBy>(x => x.CreatedBy.ToString());
        public static readonly IFieldResolver LastModified = Resolve<IEntity>(x => x.LastModified.ToString());
        public static readonly IFieldResolver LastModifiedBy = Resolve<IEntityWithLastModifiedBy>(x => x.LastModifiedBy.ToString());
        public static readonly IFieldResolver Version = Resolve<IEntityWithVersion>(x => x.Version);

        private static IFieldResolver Resolve<TSource>(Func<TSource, object> action)
        {
            return new FuncFieldResolver<TSource, object?>(c => action(c.Source));
        }
    }
}
