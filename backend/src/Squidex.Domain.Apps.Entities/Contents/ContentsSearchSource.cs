﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.Search;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentsSearchSource : ISearchSource
    {
        private readonly IAppProvider appProvider;
        private readonly IContentQueryService contentQuery;
        private readonly ITextIndex contentTextIndexer;
        private readonly IUrlGenerator urlGenerator;

        public ContentsSearchSource(
            IAppProvider appProvider,
            IContentQueryService contentQuery,
            ITextIndex contentTextIndexer,
            IUrlGenerator urlGenerator)
        {
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(contentQuery, nameof(contentQuery));
            Guard.NotNull(contentTextIndexer, nameof(contentTextIndexer));
            Guard.NotNull(urlGenerator, nameof(urlGenerator));

            this.appProvider = appProvider;
            this.contentQuery = contentQuery;
            this.contentTextIndexer = contentTextIndexer;
            this.urlGenerator = urlGenerator;
        }

        public async Task<SearchResults> SearchAsync(string query, Context context)
        {
            var result = new SearchResults();

            var searchFilter = await CreateSearchFilterAsync(context);

            if (searchFilter == null)
            {
                return result;
            }

            var ids = await contentTextIndexer.SearchAsync($"{query}~", context.App, searchFilter, context.Scope());

            if (ids == null || ids.Count == 0)
            {
                return result;
            }

            var appId = context.App.NamedId();

            var contents = await contentQuery.QueryAsync(context, ids);

            foreach (var content in contents)
            {
                var url = urlGenerator.ContentUI(appId, content.SchemaId, content.Id);

                var name = FormatName(content, context.App.LanguagesConfig.Master);

                result.Add(name, SearchResultType.Content, url, content.SchemaDisplayName);
            }

            return result;
        }

        private async Task<SearchFilter?> CreateSearchFilterAsync(Context context)
        {
            var allowedSchemas = new List<DomainId>();

            var schemas = await appProvider.GetSchemasAsync(context.App.Id);

            foreach (var schema in schemas)
            {
                if (HasPermission(context, schema.SchemaDef.Name))
                {
                    allowedSchemas.Add(schema.Id);
                }
            }

            if (allowedSchemas.Count == 0)
            {
                return null;
            }

            return SearchFilter.MustHaveSchemas(allowedSchemas.ToArray());
        }

        private static bool HasPermission(Context context, string schemaName)
        {
            var permission = Permissions.ForApp(Permissions.AppContentsRead, context.App.Name, schemaName);

            return context.Permissions.Allows(permission);
        }

        private static string FormatName(IEnrichedContentEntity content, string masterLanguage)
        {
            var sb = new StringBuilder();

            IJsonValue? GetValue(NamedContentData? data, RootField field)
            {
                if (data != null && data.TryGetValue(field.Name, out var fieldValue) && fieldValue != null)
                {
                    var isInvariant = field.Partitioning.Equals(Partitioning.Invariant);

                    if (isInvariant && fieldValue.TryGetValue("iv", out var value))
                    {
                        return value;
                    }

                    if (!isInvariant && fieldValue.TryGetValue(masterLanguage, out value))
                    {
                        return value;
                    }
                }

                return null;
            }

            if (content.ReferenceFields != null)
            {
                foreach (var field in content.ReferenceFields)
                {
                    var value = GetValue(content.ReferenceData, field) ?? GetValue(content.Data, field);

                    var formatted = StringFormatter.Format(value, field);

                    if (!string.IsNullOrWhiteSpace(formatted))
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append(", ");
                        }

                        sb.Append(formatted);
                    }
                }
            }

            if (sb.Length == 0)
            {
                return "Content";
            }

            return sb.ToString();
        }
    }
}
