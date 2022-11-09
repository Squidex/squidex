// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using GraphQLParser.AST;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Directives;

public sealed class CacheDirective : Directive
{
    public CacheDirective()
        : base("cache", DirectiveLocation.Field, DirectiveLocation.FragmentSpread, DirectiveLocation.InlineFragment)
    {
        Description = "Enable Memory Caching";

        Arguments = new QueryArguments(new QueryArgument<IntGraphType>
        {
            Name = "duration",
            Description = "Cache duration in seconds.",
            DefaultValue = 600
        });
    }
}
