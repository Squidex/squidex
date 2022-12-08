// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
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

namespace Squidex.Domain.Apps.Entities.Contents;

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
        this.appProvider = appProvider;
        this.contentQuery = contentQuery;
        this.contentTextIndexer = contentTextIndexer;
        this.urlGenerator = urlGenerator;
    }

    public async Task<SearchResults> SearchAsync(string query, Context context,
        CancellationToken ct)
    {
        var result = new SearchResults();

        var schemaIds = await GetSchemaIdsAsync(context, ct);

        if (schemaIds.Count == 0)
        {
            return result;
        }

        var textQuery = new TextQuery($"{query}~", 10)
        {
            RequiredSchemaIds = schemaIds
        };

        var ids = await contentTextIndexer.SearchAsync(context.App, textQuery, context.Scope(), ct);

        if (ids == null || ids.Count == 0)
        {
            return result;
        }

        var appId = context.App.NamedId();

        var contents = await contentQuery.QueryAsync(context, Q.Empty.WithIds(ids).WithoutTotal(), ct);

        foreach (var content in contents)
        {
            var url = urlGenerator.ContentUI(appId, content.SchemaId, content.Id);

            var name = FormatName(content, context.App.Languages.Master);

            result.Add(name, SearchResultType.Content, url, content.SchemaDisplayName);
        }

        return result;
    }

    private async Task<List<DomainId>> GetSchemaIdsAsync(Context context,
        CancellationToken ct)
    {
        var schemas = await appProvider.GetSchemasAsync(context.App.Id, ct);

        return schemas.Where(x => HasPermission(context, x.SchemaDef.Name)).Select(x => x.Id).ToList();
    }

    private static bool HasPermission(Context context, string schemaName)
    {
        return context.UserPermissions.Allows(PermissionIds.AppContentsReadOwn, context.App.Name, schemaName);
    }

    private static string FormatName(IEnrichedContentEntity content, string masterLanguage)
    {
        var sb = new StringBuilder();

        JsonValue? GetValue(ContentData? data, RootField field)
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

                var formatted = StringFormatter.Format(field, value ?? default);

                if (!string.IsNullOrWhiteSpace(formatted))
                {
                    sb.AppendIfNotEmpty(", ");
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
