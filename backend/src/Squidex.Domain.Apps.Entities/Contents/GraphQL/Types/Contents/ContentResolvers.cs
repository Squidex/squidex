// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using GraphQL.Resolvers;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;

internal static class ContentResolvers
{
    public static readonly IFieldResolver Field = Resolvers.Sync<ContentData, object?>((content, fieldContext, _) =>
    {
        var fieldName = fieldContext.FieldDefinition.SourceName();

        return content?.GetValueOrDefault(fieldName);
    });

    public static readonly IFieldResolver Url = Resolve((content, _, context) =>
    {
        var urlGenerator = context.Resolve<IUrlGenerator>();

        return urlGenerator.ContentUI(content.AppId, content.SchemaId, content.Id);
    });

    public static readonly IFieldResolver FlatData = Resolve((content, c, context) =>
    {
        var language = context.Context.App.Languages.Master;

        return content.Data.ToFlatten(language);
    });

    public static readonly IFieldResolver Data = Resolve(x => x.Data);

    public static readonly IFieldResolver ListTotal = ResolveList(x => x.Total);

    public static readonly IFieldResolver ListItems = ResolveList(x => x);

    private static IFieldResolver Resolve<T>(Func<IEnrichedContentEntity, IResolveFieldContext, GraphQLExecutionContext, T> resolver)
    {
        return Resolvers.Sync(resolver);
    }

    private static IFieldResolver Resolve<T>(Func<IEnrichedContentEntity, T> resolver)
    {
        return Resolvers.Sync(resolver);
    }

    private static IFieldResolver ResolveList<T>(Func<IResultList<IEnrichedContentEntity>, T> resolver)
    {
        return Resolvers.Sync(resolver);
    }
}
