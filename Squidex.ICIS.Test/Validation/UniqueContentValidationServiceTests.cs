using System;
using FakeItEasy;
using System.Collections.Generic;
using System.Threading.Tasks;
using NodaTime;
using Orleans;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.ICIS.Validation;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.ICIS.Test.Validation
{
    public class UniqueContentValidationServiceTests
    {
        private CreateContent content;
        private readonly Func<Task> next = A.Fake<Func<Task>>();

        private readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
        private readonly IContextProvider contextProvider = A.Fake<IContextProvider>();
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();

        private readonly Guid regionGuid = Guid.NewGuid();
        private readonly Guid commodityGuid = Guid.NewGuid();
        private readonly Guid commentaryTypeGuid = Guid.NewGuid();

        [Fact]
        public async void Should_reject_if_not_all_fields_present()
        {
            content = new CreateContent();
            CreateBrokenContentData(content);
            var context = new CommandContext(content, new InMemoryCommandBus(new List<ICommandMiddleware>()));
            var validationCommand = new UniqueContentValidationCommand(contentQuery, contextProvider, grainFactory);
           
            await Assert.ThrowsAsync<DomainException>(() => validationCommand.HandleAsync(context, next));
        }

        [Fact]
        public void Should_fail_if_existing_document_is_found()
        {
            content = new CreateContent();
            CreateWorkingContentData(content);

            var contentEntity = CreateEnrichedContent(new Guid(), new Guid(), new Guid(), null, null); 

            var context = new CommandContext(content, new InMemoryCommandBus(new List<ICommandMiddleware>()));
            A.CallTo(() => contentQuery.QueryAsync(contextProvider.Context, content.SchemaId.Name, A<Q>.Ignored)).Returns(ResultList.CreateFrom(1, contentEntity));
            var validationCommand = new UniqueContentValidationCommand(contentQuery, contextProvider, grainFactory);

            Assert.False(validationCommand.HandleAsync(context, next).IsCompletedSuccessfully);
        }

        [Fact]
        public void Should_pass_if_all_fields_present_and_unique()
        {
            content = new CreateContent();
            CreateWorkingContentData(content);
            var context = new CommandContext(content, new InMemoryCommandBus(new List<ICommandMiddleware>()));
            var validationCommand = new UniqueContentValidationCommand(contentQuery, contextProvider, grainFactory);

            Assert.True(validationCommand.HandleAsync(context, next).IsCompletedSuccessfully);
        }

        [Fact]
        public void Should_pass_if_document_from_db_has_same_id()
        {
            content = new CreateContent();
            CreateWorkingContentData(content);

            var contentEntity = CreateEnrichedContent(content.ContentId, new Guid(), new Guid(), null, null);

            var context = new CommandContext(content, new InMemoryCommandBus(new List<ICommandMiddleware>()));
            A.CallTo(() => contentQuery.QueryAsync(contextProvider.Context, content.SchemaId.Name, A<Q>.Ignored)).Returns(ResultList.CreateFrom(1, contentEntity));
            var validationCommand = new UniqueContentValidationCommand(contentQuery, contextProvider, grainFactory);

            Assert.True(validationCommand.HandleAsync(context, next).IsCompletedSuccessfully);
        }

        private static void CreateBrokenContentData(CreateContent content)
        {
            var data = new NamedContentData {
                { "region", null},
                { "commodity", new ContentFieldData()},
                { "commentarytype", new ContentFieldData()},
                { "createdfor", new ContentFieldData()},
                {"iv", new ContentFieldData()}
            };             

            content.Data = data;
        }

        private void CreateWorkingContentData(CreateContent content)
        {
            var currentTime = NodaTime.Instant.FromDateTimeUtc(DateTime.Now.ToUniversalTime());
            var value = JsonValue.Create(currentTime);

            content.SchemaId = new NamedId<Guid>(Guid.NewGuid(), "test");

            var data = new NamedContentData {
                { "region", new ContentFieldData().AddJsonValue("iv", new JsonArray {JsonValue.Create(regionGuid.ToString())})},
                { "commodity", new ContentFieldData().AddJsonValue("iv",new JsonArray {JsonValue.Create(commodityGuid.ToString())})},
                { "commentarytype", new ContentFieldData().AddJsonValue("iv",new JsonArray {JsonValue.Create(commentaryTypeGuid.ToString())})},
                { "createdfor", new ContentFieldData().AddJsonValue("iv", value)}
            };

            content.Data = data;
        }

        private static IEnrichedContentEntity CreateEnrichedContent(Guid id, Guid refId, Guid assetId, NamedContentData data = null, NamedContentData dataDraft = null)
        {
            var now = SystemClock.Instance.GetCurrentInstant();

            data = data ??
                new NamedContentData()
                    .AddField("my-string",
                        new ContentFieldData()
                            .AddValue("de", "value"))
                    .AddField("my-assets",
                        new ContentFieldData()
                            .AddValue("iv", JsonValue.Array(assetId.ToString())))
                    .AddField("my-number",
                        new ContentFieldData()
                            .AddValue("iv", 1.0))
                    .AddField("my_number",
                        new ContentFieldData()
                            .AddValue("iv", 2.0))
                    .AddField("my-boolean",
                        new ContentFieldData()
                            .AddValue("iv", true))
                    .AddField("my-datetime",
                        new ContentFieldData()
                            .AddValue("iv", now))
                    .AddField("my-tags",
                        new ContentFieldData()
                            .AddValue("iv", JsonValue.Array("tag1", "tag2")))
                    .AddField("my-references",
                        new ContentFieldData()
                            .AddValue("iv", JsonValue.Array(refId.ToString())))
                    .AddField("my-geolocation",
                        new ContentFieldData()
                            .AddValue("iv", JsonValue.Object().Add("latitude", 10).Add("longitude", 20)))
                    .AddField("my-json",
                        new ContentFieldData()
                            .AddValue("iv", JsonValue.Object().Add("value", 1)))
                    .AddField("my-localized",
                        new ContentFieldData()
                            .AddValue("de-DE", "de-DE"))
                    .AddField("my-array",
                        new ContentFieldData()
                            .AddValue("iv", JsonValue.Array(
                                JsonValue.Object()
                                    .Add("nested-boolean", true)
                                    .Add("nested-number", 10)
                                    .Add("nested_number", 11),
                                JsonValue.Object()
                                    .Add("nested-boolean", false)
                                    .Add("nested-number", 20)
                                    .Add("nested_number", 21))));

            var content = new ContentEntity
            {
                Id = id,
                Version = 1,
                Created = now,
                CreatedBy = new RefToken(RefTokenType.Subject, "user1"),
                LastModified = now,
                LastModifiedBy = new RefToken(RefTokenType.Subject, "user2"),
                Data = data,
                DataDraft = dataDraft,
                Status = Status.Draft,
                StatusColor = "red"
            };

            return content;
        }

    }
}
