// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using GraphQL.Types;
using GraphQLParser.AST;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Directives;

public sealed class OptimizeFieldQueriesDirective : Directive
{
    public OptimizeFieldQueriesDirective()
        : base("optimizeFieldQueries", DirectiveLocation.Field, DirectiveLocation.FragmentSpread, DirectiveLocation.InlineFragment)
    {
        Description = "Enable Query Optimizations";
    }

    public static bool IsApplied(IResolveFieldContext context)
    {
        return context.GetDirective("optimizeFieldQueries") != null;
    }
}
