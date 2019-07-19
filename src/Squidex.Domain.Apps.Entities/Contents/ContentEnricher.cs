// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public sealed class ContentEnricher : IContentEnricher
    {
        private const string DefaultColor = StatusColors.Draft;
        private readonly IAppProvider appProvider;
        private readonly IContentQueryService contentQuery;
        private readonly IContentWorkflow contentWorkflow;
        private readonly IContextProvider contextProvider;

        public ContentEnricher(
            IAppProvider appProvider,
            IContentQueryService contentQuery,
            IContentWorkflow contentWorkflow,
            IContextProvider contextProvider)
        {
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(contentQuery, nameof(contentQuery));
            Guard.NotNull(contentWorkflow, nameof(contentWorkflow));
            Guard.NotNull(contextProvider, nameof(contextProvider));

            this.appProvider = appProvider;
            this.contentQuery = contentQuery;
            this.contentWorkflow = contentWorkflow;
            this.contextProvider = contextProvider;
        }

        public async Task<IEnrichedContentEntity> EnrichAsync(IContentEntity content, ClaimsPrincipal user)
        {
            Guard.NotNull(content, nameof(content));

            var enriched = await EnrichAsync(Enumerable.Repeat(content, 1), user);

            return enriched[0];
        }

        public async Task<IReadOnlyList<IEnrichedContentEntity>> EnrichAsync(IEnumerable<IContentEntity> contents, ClaimsPrincipal user)
        {
            Guard.NotNull(contents, nameof(contents));
            Guard.NotNull(user, nameof(user));

            using (Profiler.TraceMethod<ContentEnricher>())
            {
                var results = new List<ContentEntity>();

                if (contents.Any())
                {
                    var cache = new Dictionary<(Guid, Status), StatusInfo>();

                    foreach (var content in contents)
                    {
                        var result = SimpleMapper.Map(content, new ContentEntity());

                        await ResolveColorAsync(content, result, cache);

                        if (ShouldEnrichWithStatuses())
                        {
                            await ResolveNextsAsync(content, result, user);
                            await ResolveCanUpdateAsync(content, result);
                        }

                        results.Add(result);
                    }

                    if (contextProvider.Context.IsFrontendClient)
                    {
                        foreach (var group in results.GroupBy(x => x.SchemaId))
                        {
                            await ResolveReferencesAsync(group.Key, group);
                        }
                    }
                }

                return results;
            }
        }

        private async Task ResolveReferencesAsync(NamedId<Guid> schemaId, IEnumerable<ContentEntity> contents)
        {
            var appId = contents.First().AppId.Id;

            var schema = await appProvider.GetSchemaAsync(appId, schemaId.Id);

            var referenceFields =
                schema.SchemaDef.Fields.OfType<IField<ReferencesFieldProperties>>()
                    .Where(x =>
                        x.Properties.MinItems == 1 &&
                        x.Properties.MaxItems == 1 &&
                        (x.Properties.IsListField || x.Properties.IsReferenceField));

            foreach (var field in referenceFields)
            {
                var allIds = GetContentIds(contents, field);

                if (allIds.Count > 0)
                {
                    var referenced = await contentQuery.QueryAsync(contextProvider.Context, schemaId.Id.ToString(), Q.Empty.WithIds(allIds));

                    var byId = referenced.ToDictionary(x => x.Id);

                    foreach (var content in contents)
                    {
                        content.ReferenceData = content.ReferenceData ?? new NamedContentData();

                        if (content.DataDraft.TryGetValue(field.Name, out var fieldData))
                        {
                            foreach (var partitionValue in fieldData.Where(x => x.Value.Type != JsonValueType.Null))
                            {
                                var ids = field.GetReferencedIds(partitionValue.Value).ToArray();

                                if (ids.Length == 1)
                                {
                                    if (byId.TryGetValue(ids[0], out var reference))
                                    {
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private static HashSet<Guid> GetContentIds(IEnumerable<ContentEntity> contents, IField<ReferencesFieldProperties> field)
        {
            var allIds = new HashSet<Guid>();

            foreach (var content in contents)
            {
                allIds.AddRange(content.DataDraft.GetReferencedIds(field));
            }

            return allIds;
        }

        private bool ShouldEnrichWithStatuses()
        {
            return contextProvider.Context.IsFrontendClient || contextProvider.Context.IsResolveFlow();
        }

        private async Task ResolveCanUpdateAsync(IContentEntity content, ContentEntity result)
        {
            result.CanUpdate = await contentWorkflow.CanUpdateAsync(content);
        }

        private async Task ResolveNextsAsync(IContentEntity content, ContentEntity result, ClaimsPrincipal user)
        {
            result.Nexts = await contentWorkflow.GetNextsAsync(content, user);
        }

        private async Task ResolveColorAsync(IContentEntity content, ContentEntity result, Dictionary<(Guid, Status), StatusInfo> cache)
        {
            result.StatusColor = await GetColorAsync(content, cache);
        }

        private async Task<string> GetColorAsync(IContentEntity content, Dictionary<(Guid, Status), StatusInfo> cache)
        {
            if (!cache.TryGetValue((content.SchemaId.Id, content.Status), out var info))
            {
                info = await contentWorkflow.GetInfoAsync(content);

                if (info == null)
                {
                    info = new StatusInfo(content.Status, DefaultColor);
                }

                cache[(content.SchemaId.Id, content.Status)] = info;
            }

            return info.Color;
        }
    }
}
