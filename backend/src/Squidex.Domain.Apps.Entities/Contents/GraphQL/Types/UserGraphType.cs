// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;

internal sealed class UserGraphType : SharedObjectGraphType<IUser>
{
    public static readonly IGraphType Nullable = new UserGraphType();

    public static readonly IGraphType NonNull = new NonNullGraphType(Nullable);

    public UserGraphType()
    {
        // The name is used for equal comparison. Therefore it is important to treat it as readonly.
        Name = "User";

        AddField(new FieldType
        {
            Name = "id",
            Resolver = Resolve(x => x.Id),
            ResolvedType = Scalars.NonNullString,
            Description = FieldDescriptions.UserId
        });

        AddField(new FieldType
        {
            Name = "displayName",
            Resolver = Resolve(x => x.Claims.DisplayName()),
            ResolvedType = Scalars.String,
            Description = FieldDescriptions.UserDisplayName
        });

        AddField(new FieldType
        {
            Name = "email",
            Resolver = Resolve(x => x.Email),
            ResolvedType = Scalars.String,
            Description = FieldDescriptions.UserEmail
        });

        Description = "A user that created or modified a content or asset.";
    }

    private static IFieldResolver Resolve<T>(Func<IUser, T> resolver)
    {
        return Resolvers.Sync(resolver);
    }
}
