// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL.Resolvers;
using Squidex.Infrastructure;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives
{
    internal static class EntityResolvers
    {
        public static readonly IFieldResolver Id = Resolve<IEntity>(x => x.Id.ToString());
        public static readonly IFieldResolver Created = Resolve<IEntity>(x => x.Created.ToDateTimeUtc());
        public static readonly IFieldResolver CreatedBy = Resolve<IEntityWithCreatedBy>(x => x.CreatedBy.ToString());
        public static readonly IFieldResolver CreatedByUser = ResolveUser<IEntityWithCreatedBy>(x => x.CreatedBy);
        public static readonly IFieldResolver LastModified = Resolve<IEntity>(x => x.LastModified.ToDateTimeUtc());
        public static readonly IFieldResolver LastModifiedBy = Resolve<IEntityWithLastModifiedBy>(x => x.LastModifiedBy.ToString());
        public static readonly IFieldResolver LastModifiedByUser = ResolveUser<IEntityWithLastModifiedBy>(x => x.LastModifiedBy);
        public static readonly IFieldResolver Version = Resolve<IEntityWithVersion>(x => x.Version);

        private static IFieldResolver Resolve<TSource>(Func<TSource, object> resolver)
        {
            return Resolvers.Sync(resolver);
        }

        private static IFieldResolver ResolveUser<TSource>(Func<TSource, RefToken> resolver)
        {
            return Resolvers.Async<TSource, IUser>((source, fieldContext, context) => context.FindUserAsync(resolver(source), fieldContext.CancellationToken));
        }
    }
}
