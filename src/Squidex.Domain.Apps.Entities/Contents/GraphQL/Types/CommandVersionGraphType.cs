// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class CommandVersionGraphType : ObjectGraphType<CommandContext>
    {
        public CommandVersionGraphType()
        {
            Name = "CommandVersionDto";

            AddField(new FieldType
            {
                Name = "version",
                ResolvedType = AllTypes.Int,
                Resolver = ResolveVersion(),
                Description = "The new version of the item."
            });

            Description = "The result of a mutation";
        }

        private static IFieldResolver ResolveVersion()
        {
            return new FuncFieldResolver<CommandContext, int?>(x =>
            {
                if (x.Source.Result<object>() is EntitySavedResult result)
                {
                    return (int)result.Version;
                }

                return null;
            });
        }
    }
}
