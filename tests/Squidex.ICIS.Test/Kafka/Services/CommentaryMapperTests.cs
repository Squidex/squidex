using FakeItEasy;
using FluentAssertions;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.ICIS.Kafka.Entities;
using Squidex.ICIS.Kafka.Services;
using Squidex.Infrastructure.Json.Objects;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Squidex.ICIS.Test.Kafka.Services
{
    public class CommentaryMapperTests
    {
        private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
        private readonly IAppEntity app = A.Fake<IAppEntity>();

        [Fact]
        public async Task Should_map_commentary_from_value()
        {
            var commentaryTypeId = Guid.NewGuid();
            var commentaryType = CreateRef(commentaryTypeId, "ref/commentarytype/1");
            var commodityId = Guid.NewGuid();
            var commodity = CreateRef(commodityId, "ref/commodity/1");
            var regionId = Guid.NewGuid();
            var region = CreateRef(regionId, "ref/region/1");

            var data =
                new NamedContentData()
                    .AddField("body",
                        new ContentFieldData()
                            .AddValue("en", "my-body"))
                    .AddField("commentarytype",
                        new ContentFieldData()
                            .AddValue(JsonValue.Array(commentaryTypeId.ToString())))
                    .AddField("commodity",
                        new ContentFieldData()
                            .AddValue(JsonValue.Array(commodityId.ToString())))
                    .AddField("region",
                        new ContentFieldData()
                            .AddValue(JsonValue.Array(regionId.ToString())))
                    .AddField("createdfor",
                        new ContentFieldData()
                            .AddValue(Instant.FromUnixTimeSeconds(1200).ToString()));

            A.CallTo(() => contentRepository.QueryAsync(app, A<Status[]>.That.IsSameSequenceAs(Status.Published, Status.Draft),
                    A<HashSet<Guid>>.That.Matches(x =>
                        x.Count == 3 &&
                        x.Contains(commentaryTypeId) &&
                        x.Contains(commodityId) &&
                        x.Contains(regionId)),
                    true))
                .Returns(new List<(IContentEntity Content, ISchemaEntity Schema)>
                {
                    commentaryType,
                    commodity,
                    region
                });

            var @event = new EnrichedContentEvent
            {
                Id = Guid.NewGuid(),
                Data = data,
                LastModified = Instant.FromUnixTimeSeconds(5300)
            };

            var result = await CommentaryMapper.ToAvroAsync(@event, app, contentRepository);

            result.Should().BeEquivalentTo(new Commentary
            {
                Id = @event.Id.ToString(),
                Body = "my-body",
                CreatedFor = 1200,
                CommentaryTypeId = "ref/commentarytype/1",
                CommodityId = "ref/commodity/1",
                LastModified = 5300,
                RegionId = "ref/region/1"
            });
        }

        private (IContentEntity Content, ISchemaEntity Schema) CreateRef(Guid id, string refId)
        {
            var data =
                new NamedContentData()
                    .AddField("id",
                        new ContentFieldData()
                            .AddValue(refId));

            var content = A.Fake<IContentEntity>();

            A.CallTo(() => content.Id).Returns(id);
            A.CallTo(() => content.Data).Returns(data);

            return (content, null);
        }
    }
}
