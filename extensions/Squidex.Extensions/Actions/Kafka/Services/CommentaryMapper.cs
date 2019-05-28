// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avro.Specific;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Extensions.Actions.Kafka.Entities;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Extensions.Actions.Kafka
{
    public class CommentaryMapper : IKafkaMessageMapper
    {
        private IContentRepository contentRepository;
        private IAppProvider appProvider;
        public CommentaryMapper(IAppProvider appProvider, IContentRepository contentRepository)
        {
            this.appProvider = appProvider;
            this.contentRepository = contentRepository;
        }

        public ISpecificRecord ToAvro(NamedContentData namedContentData)
        {
            var commentary = new Commentary();

            if (!namedContentData.TryGetValue("Body", out var bodyData))
            {
                throw new System.Exception("Unable to find Body field.");
            }

            commentary.Body = bodyData["en"].ToString();

            if (!namedContentData.TryGetValue("CommentaryType", out var commentaryTypeData))
            {
                throw new System.Exception("Unable to find CommentaryType field.");
            }

            if (!namedContentData.TryGetValue("Commodity", out var commodityData))
            {
                throw new System.Exception("Unable to find Commodity field.");
            }

            var app = appProvider.GetAppAsync("commentary");
            app.Wait();

            var commentaryTypeId = ((Collection<IJsonValue>)commentaryTypeData["iv"])[0].ToString();
            var commodityId = ((Collection<IJsonValue>)commodityData["iv"])[0].ToString();

            var commentaryType = GetPublishedEntity("commentary-type", commentaryTypeId, app.Result);
            var commodity = GetPublishedEntity("commodity", commodityId, app.Result);

            commentary.CommentaryType = (CommentaryType)new CommentaryTypeMapper().ToAvro(commentaryType.Data);
            commentary.Commodity = (Commodity)new CommodityMapper().ToAvro(commodity.Data);

            return commentary;
        }

        private IContentEntity GetPublishedEntity(string schemaName, string entityId, IAppEntity app)
        {
            var schema = appProvider.GetSchemaAsync(app.Id, schemaName);
            schema.Wait();

            var entity = this.contentRepository.FindContentAsync(app, schema.Result, new Status[] { Status.Published }, new System.Guid(entityId), false);
            entity.Wait();

            return entity.Result;
        }
    }
}
