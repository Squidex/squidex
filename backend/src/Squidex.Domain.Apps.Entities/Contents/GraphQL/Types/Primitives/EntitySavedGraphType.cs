// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives
{
    internal sealed class EntitySavedGraphType : ObjectGraphType<CommandResult>
    {
        public static readonly IGraphType Nullable = new EntitySavedGraphType();

        public static readonly IGraphType NonNull = new NonNullGraphType(Nullable);

        private EntitySavedGraphType()
        {
            // The name is used for equal comparison. Therefore it is important to treat it as readonly.
            Name = "EntitySavedResultDto";

            AddField(new FieldType
            {
                Name = "version",
                Resolver = ResolveVersion(),
                ResolvedType = AllTypes.NonNullLong,
                Description = "The new version of the item."
            });

            Description = "The result of a mutation";
        }

        private static IFieldResolver ResolveVersion()
        {
            return Resolvers.Sync<CommandResult, long>(x => x.NewVersion);
        }
    }
}
