// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using GraphQL;
using GraphQL.Resolvers;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents
{
    internal static class ContentResolvers
    {
        public static IFieldResolver NestedValue(ValueResolver valueResolver, string key)
        {
            return Resolvers.Sync<JsonObject, object?>((source, fieldContext, context) =>
            {
                if (source.TryGetValue(key, out var value))
                {
                    return valueResolver(value, fieldContext, context);
                }

                return null;
            });
        }

        public static IFieldResolver Partition(ValueResolver valueResolver, string key)
        {
            return Resolvers.Sync<ContentFieldData, object?>((source, fieldContext, context) =>
            {
                if (source.TryGetValue(key, out var value) && value != null)
                {
                    return valueResolver(value, fieldContext, context);
                }

                return null;
            });
        }

        public static IFieldResolver FlatPartition(ValueResolver valueResolver, string key)
        {
            return Resolvers.Sync<FlatContentData, object?>((source, fieldContext, context) =>
            {
                if (source.TryGetValue(key, out var value) && value != null)
                {
                    return valueResolver(value, fieldContext, context);
                }

                return null;
            });
        }

        public static IFieldResolver Field(RootField field)
        {
            var fieldName = field.Name;

            return Resolvers.Sync<ContentData, IReadOnlyDictionary<string, IJsonValue>?>(source =>
            {
                return source?.GetOrDefault(fieldName);
            });
        }

        public static readonly IFieldResolver Url = Resolve((content, _, context) =>
        {
            var appId = content.AppId;

            return context.UrlGenerator.ContentUI(appId, content.SchemaId, content.Id);
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
}
