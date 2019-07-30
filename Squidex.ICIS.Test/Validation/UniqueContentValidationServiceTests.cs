using System;
using FakeItEasy;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.ICIS.Validation;
using Squidex.ICIS.Test.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.ICIS.Test.Validation
{
    public class UniqueContentValidationServiceTests
    {
        private readonly TestHelper testHelper;

        private readonly Func<Task> next = A.Fake<Func<Task>>();
        private readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
        private readonly IContextProvider contextProvider = A.Fake<IContextProvider>();
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();

        private readonly Guid regionGuid = Guid.NewGuid();
        private readonly Guid commodityGuid = Guid.NewGuid();
        private readonly Guid commentaryTypeGuid = Guid.NewGuid();

        public UniqueContentValidationServiceTests()
        {
            testHelper = new TestHelper();
        }

        [Fact]
        public async void Should_reject_if_not_all_fields_present()
        {
            var content = new CreateContent();
            CreateBrokenContentData(content);
            var context = new CommandContext(content, new InMemoryCommandBus(new List<ICommandMiddleware>()));
            var validationCommand = new UniqueContentValidationCommand(contentQuery, contextProvider, grainFactory);
           
            await Assert.ThrowsAsync<NullReferenceException>(() => validationCommand.HandleAsync(context, next));
        }

        [Fact]
        public async void Should_fail_if_existing_document_is_found()
        {
            var content = new CreateContent();
            CreateWorkingContentData(content);

            var contentEntity = testHelper.CreateEnrichedContent(new Guid(), new Guid(), new Guid(), null, null); 

            var context = new CommandContext(content, new InMemoryCommandBus(new List<ICommandMiddleware>()));
            var validationCommand = new UniqueContentValidationCommand(contentQuery, contextProvider, grainFactory);

            A.CallTo(() => contentQuery.QueryAsync(contextProvider.Context, content.SchemaId.Name, A<Q>.Ignored)).Returns(ResultList.CreateFrom(1, contentEntity));

            await Assert.ThrowsAsync<DomainException>(() => validationCommand.HandleAsync(context, next));
            Assert.False(validationCommand.HandleAsync(context, next).IsCompletedSuccessfully);

        }

        [Fact]
        public void Should_pass_if_all_fields_present_and_unique()
        {
            var content = new CreateContent();
            CreateWorkingContentData(content);

            var context = new CommandContext(content, new InMemoryCommandBus(new List<ICommandMiddleware>()));
            var validationCommand = new UniqueContentValidationCommand(contentQuery, contextProvider, grainFactory);

            Assert.True(validationCommand.HandleAsync(context, next).IsCompletedSuccessfully);
        }

        [Fact]
        public void Should_pass_if_document_from_db_has_same_id()
        {
            var content = new CreateContent();
            CreateWorkingContentData(content);

            var contentEntity = testHelper.CreateEnrichedContent(content.ContentId, new Guid(), new Guid(), null, null);

            var context = new CommandContext(content, new InMemoryCommandBus(new List<ICommandMiddleware>())); 
            var validationCommand = new UniqueContentValidationCommand(contentQuery, contextProvider, grainFactory);

            A.CallTo(() => contentQuery.QueryAsync(contextProvider.Context, content.SchemaId.Name, A<Q>.Ignored)).Returns(ResultList.CreateFrom(1, contentEntity));

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

    }
}
