// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avro.Specific;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.ICIS.Kafka.Entities;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.ICIS.Kafka.Services
{
    public class CommentaryMapper : IKafkaMessageMapper
    {
        private IContentRepository contentRepository;
        private readonly IAppEntity commentaryApp;

        public CommentaryMapper(IAppEntity commentaryApp, IContentRepository contentRepository)
        {
            this.commentaryApp = commentaryApp;
            this.contentRepository = contentRepository;
        }

        public ISpecificRecord ToAvro(EnrichedContentEvent contentEvent)
        {
            var commentary = new Commentary();
            commentary.Id = contentEvent.Id.ToString();
            commentary.LastModified = contentEvent.LastModified.ToUnixTimeSeconds();
            commentary.CreatedFor = contentEvent.Created.ToUnixTimeSeconds();

            if (!contentEvent.Data.TryGetValue("Body", out var bodyData))
            {
                throw new Exception("Unable to find Body field.");
            }

            commentary.Body = bodyData["en"]?.ToString();

            if (!contentEvent.Data.TryGetValue("CommentaryType", out var commentaryTypeData))
            {
                throw new Exception("Unable to find CommentaryType field.");
            }

            if (!contentEvent.Data.TryGetValue("Commodity", out var commodityData))
            {
                throw new Exception("Unable to find Commodity field.");
            }

            if (!contentEvent.Data.TryGetValue("Region", out var regionData))
            {
                throw new Exception("Unable to find Region field.");
            }

            var commentaryTypeDBId = ((Collection<IJsonValue>)commentaryTypeData["iv"])[0].ToString();
            var commodityDBId = ((Collection<IJsonValue>)commodityData["iv"])[0].ToString();
            var regionDBId = ((Collection<IJsonValue>)regionData["iv"])[0].ToString();

            var publishedEntities = GetPublishedEntities(string.Join(',', new[] { commentaryTypeDBId, commodityDBId, regionDBId }));

            var commentaryType = publishedEntities.Find(x => x.Item2.SchemaDef.Name.Equals("commentary-type")).Item1;
            var commodity = publishedEntities.Find(x => x.Item2.SchemaDef.Name.Equals("commodity")).Item1;
            var region = publishedEntities.Find(x => x.Item2.SchemaDef.Name.Equals("region")).Item1;

            if (!commentaryType.Data.TryGetValue("ID", out var commentaryTypeIdData))
            {
                throw new Exception("Unable to find commentary-type Id field.");
            }

            if (!commodity.Data.TryGetValue("ID", out var commodityIdData))
            {
                throw new Exception("Unable to find commodity Id field.");
            }

            if (!region.Data.TryGetValue("ID", out var regionIdData))
            {
                throw new Exception("Unable to find commodity Id field.");
            }

            commentary.CommentaryTypeId = commentaryTypeIdData["iv"].ToString();
            commentary.CommodityId = commodityIdData["iv"].ToString();
            commentary.RegionId = regionIdData["iv"].ToString();

            return commentary;
        }

        private List<(IContentEntity, ISchemaEntity)> GetPublishedEntities(string entityIds)
        {
            var entity = this.contentRepository.QueryAsync(this.commentaryApp, new Status[] { Status.Published }, new HashSet<Guid>(Q.Empty.WithIds(entityIds).Ids), false);
            entity.Wait();

            return entity.Result;
        }
    }
}
