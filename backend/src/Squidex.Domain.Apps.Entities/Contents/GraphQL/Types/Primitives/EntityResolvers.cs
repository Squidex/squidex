// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Resolvers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives;

internal static class EntityResolvers
{
    public static readonly IFieldResolver Id = Resolve<Entity>(x => x.Id.ToString());
    public static readonly IFieldResolver Created = Resolve<Entity>(x => x.Created.ToDateTimeUtc());
    public static readonly IFieldResolver CreatedBy = Resolve<Entity>(x => x.CreatedBy.ToString());
    public static readonly IFieldResolver CreatedByUser = ResolveUser<Entity>(x => x.CreatedBy);
    public static readonly IFieldResolver LastModified = Resolve<Entity>(x => x.LastModified.ToDateTimeUtc());
    public static readonly IFieldResolver LastModifiedBy = Resolve<Entity>(x => x.LastModifiedBy.ToString());
    public static readonly IFieldResolver LastModifiedByUser = ResolveUser<Entity>(x => x.LastModifiedBy);
    public static readonly IFieldResolver Version = Resolve<Entity>(x => x.Version);

    private static IFieldResolver Resolve<TSource>(Func<TSource, object> resolver)
    {
        return Resolvers.Sync(resolver);
    }

    private static IFieldResolver ResolveUser<TSource>(Func<TSource, RefToken> resolver)
    {
        return Resolvers.Async<TSource, IUser?>((source, fieldContext, context) =>
        {
            var token = resolver(source);

            if (fieldContext.HasOnlyIdField())
            {
                return new ValueTask<IUser?>(new ClientUser(token));
            }

            return context.FindUserAsync(token, fieldContext.CancellationToken);
        });
    }
}
