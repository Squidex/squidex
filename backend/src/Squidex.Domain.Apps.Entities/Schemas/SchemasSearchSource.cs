// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Search;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Shared;

namespace Squidex.Domain.Apps.Entities.Schemas
{
    public sealed class SchemasSearchSource : ISearchSource
    {
        private readonly IAppProvider appProvider;
        private readonly IUrlGenerator urlGenerator;

        public SchemasSearchSource(IAppProvider appProvider, IUrlGenerator urlGenerator)
        {
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(urlGenerator, nameof(urlGenerator));

            this.appProvider = appProvider;

            this.urlGenerator = urlGenerator;
        }

        public async Task<SearchResults> SearchAsync(string query, Context context)
        {
            var result = new SearchResults();

            var schemas = await appProvider.GetSchemasAsync(context.App.Id);

            if (schemas.Count > 0)
            {
                var appId = context.App.NamedId();

                foreach (var schema in schemas)
                {
                    var schemaId = schema.NamedId();

                    var name = schema.SchemaDef.DisplayNameUnchanged();

                    if (name.Contains(query))
                    {
                        AddSchemaUrl(result, appId, schemaId, name);

                        if (HasPermission(context, schemaId))
                        {
                            AddContentsUrl(result, appId, schema, schemaId, name);
                        }
                    }
                }
            }

            return result;
        }

        private void AddSchemaUrl(SearchResults result, NamedId<Guid> appId, NamedId<Guid> schemaId, string name)
        {
            var schemaUrl = urlGenerator.SchemaUI(appId, schemaId);

            result.Add(T.Get("search.schemaResult", new { name }), SearchResultType.Schema, schemaUrl);
        }

        private void AddContentsUrl(SearchResults result, NamedId<Guid> appId, ISchemaEntity schema, NamedId<Guid> schemaId, string name)
        {
            if (schema.SchemaDef.IsSingleton)
            {
                var contentUrl = urlGenerator.ContentUI(appId, schemaId, schemaId.Id);

                result.Add(T.Get("search.contentResult", new { name }), SearchResultType.Content, contentUrl, name);
            }
            else
            {
                var contentUrl = urlGenerator.ContentsUI(appId, schemaId);

                result.Add(T.Get("search.contentsResult", new { name }), SearchResultType.Content, contentUrl, name);
            }
        }

        private static bool HasPermission(Context context, NamedId<Guid> schemaId)
        {
            var permission = Permissions.ForApp(Permissions.AppContentsRead, context.App.Name, schemaId.Name);

            return context.Permissions.Allows(permission);
        }
    }
}
