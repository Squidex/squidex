﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    internal sealed class UserGraphType : ObjectGraphType<IUser>
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
                ResolvedType = AllTypes.NonNullString,
                Description = "The id of the user."
            });

            AddField(new FieldType
            {
                Name = "displayName",
                Resolver = Resolve(x => x.Claims.DisplayName()),
                ResolvedType = AllTypes.String,
                Description = "The display name of the user."
            });

            AddField(new FieldType
            {
                Name = "email",
                Resolver = Resolve(x => x.Email),
                ResolvedType = AllTypes.String,
                Description = "The email of the user."
            });

            Description = "A user that created or modified a content or asset.";
        }

        private static IFieldResolver Resolve<T>(Func<IUser, T> resolver)
        {
            return Resolvers.Sync(resolver);
        }
    }
}
