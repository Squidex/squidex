// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.Search;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentsSearchSource : ISearchSource
    {
        private readonly IContentQueryService contentQuery;
        private readonly IContentTextIndexer contentTextIndexer;
        private readonly IUrlGenerator urlGenerator;

        public ContentsSearchSource(IContentQueryService contentQuery, IContentTextIndexer contentTextIndexer, IUrlGenerator urlGenerator)
        {
            Guard.NotNull(contentQuery);
            Guard.NotNull(contentTextIndexer);
            Guard.NotNull(urlGenerator);

            this.contentQuery = contentQuery;
            this.contentTextIndexer = contentTextIndexer;
            this.urlGenerator = urlGenerator;
        }

        public async Task<SearchResults> SearchAsync(string query, Context context)
        {
            var result = new SearchResults();

            var ids = await contentTextIndexer.SearchAsync($"{query}~", context.App, default, context.Scope());

            if (ids?.Count > 0)
            {
                var appId = context.App.NamedId();

                var contents = await contentQuery.QueryAsync(context, ids);

                foreach (var content in contents)
                {
                    var url = urlGenerator.ContentUI(appId, content.SchemaId, content.Id);

                    var name = FormatName(content, context.App.LanguagesConfig.Master);

                    result.Add(name, SearchResultType.Content, url, content.SchemaDisplayName);
                }
            }

            return result;
        }

        private string FormatName(IEnrichedContentEntity content, string masterLanguage)
        {
            var sb = new StringBuilder();

            IJsonValue? GetValue(NamedContentData? data, RootField field)
            {
                if (data != null && data.TryGetValue(field.Name, out var fieldValue) && fieldValue != null)
                {
                    if (fieldValue.TryGetValue("iv", out var value))
                    {
                        return value;
                    }

                    if (fieldValue.TryGetValue(masterLanguage, out value))
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
                            sb.Append(formatted);
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
