using FakeItEasy;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.ICIS.Kafka.Consumer;
using Squidex.ICIS.Kafka.Entities;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Domain.Apps.Core.Schemas;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Squidex.Domain.Apps.Entities.Contents.Commands;

namespace Squidex.ICIS.Test.Kafka.Consumer
{
    public class JsonKafkaHandlerTests
    {
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly IAppEntity app = A.Fake<IAppEntity>();
        private readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        private readonly IContentQueryService contentQueryService = A.Fake<IContentQueryService>();
        private readonly RefToken actor = new RefToken(RefTokenType.Subject, "12");
        private readonly Context context;
        private readonly JsonKafkaHandler sut;

        public sealed class FakeEntity : IRefDataEntity
        {
            public string Id => "123";

            public string IdField => "id";

            public string Schema => "schema";

            public NamedContentData Data { get; } = new NamedContentData();

            public NamedContentData ToData()
            {
                return Data;
            }
        }

        public JsonKafkaHandlerTests()
        {
            context = new Context(new ClaimsPrincipal(), app);

            sut = new JsonKafkaHandler(commandBus, appProvider, contentQueryService);
        }

        [Fact]
        public async Task Should_throw_exception_if_schema_not_found()
        {
            var entity = new FakeEntity();

            A.CallTo(() => appProvider.GetSchemaAsync(context.App.Id, entity.Schema))
                .Returns(Task.FromResult<ISchemaEntity>(null));

            await Assert.ThrowsAsync<InvalidOperationException>(() => sut.HandleAsync(actor, context, entity.Id, entity));
        }

        [Fact]
        public async Task Should_publish_create_command_if_content_not_found()
        {
            var entity = new FakeEntity();

            var schema = CreateSchema(Guid.NewGuid(), entity.Schema);

            A.CallTo(() => appProvider.GetSchemaAsync(context.App.Id, entity.Schema))
                .Returns(schema);

            A.CallTo(() => contentQueryService.QueryAsync(context, entity.Schema, A<Q>.That.Matches(x => x.ODataQuery == "$filter=data/id/iv eq '123'")))
                .Returns(ResultList.CreateFrom<IEnrichedContentEntity>(0));

            await sut.HandleAsync(actor, context, entity.Id, entity);

            var isCommand = new Func<ICommand, bool>(command =>
            {
                return
                    command is CreateContent create &&
                    create.Data == entity.Data &&
                    create.Actor == actor &&
                    create.SchemaId.Equals(schema.NamedId()) &&
                    create.Publish;
            });

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>.That.Matches(isCommand, "Command")))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_publish_update_command_if_content_found()
        {
            var entity = new FakeEntity();

            var schema = CreateSchema(Guid.NewGuid(), entity.Schema);

            var content = CreateContent(Guid.NewGuid());

            A.CallTo(() => appProvider.GetSchemaAsync(context.App.Id, entity.Schema))
                .Returns(schema);

            A.CallTo(() => contentQueryService.QueryAsync(context, entity.Schema, A<Q>.That.Matches(x => x.ODataQuery == "$filter=data/id/iv eq '123'")))
                .Returns(ResultList.CreateFrom<IEnrichedContentEntity>(1, content));

            await sut.HandleAsync(actor, context, entity.Id, entity);

            var isCommand = new Func<ICommand, bool>(command =>
            {
                return
                    command is UpdateContent create &&
                    create.Data == entity.Data &&
                    create.Actor == actor &&
                    create.ContentId == content.Id;
            });

            A.CallTo(() => commandBus.PublishAsync(A<ICommand>.That.Matches(isCommand, "Command")))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_cache_schema_id()
        {
            var entity1 = new FakeEntity();
            var entity2 = new FakeEntity();

            var schema = CreateSchema(Guid.NewGuid(), entity1.Schema);

            A.CallTo(() => appProvider.GetSchemaAsync(context.App.Id, entity1.Schema))
                .Returns(schema);

            A.CallTo(() => contentQueryService.QueryAsync(context, entity1.Schema, A<Q>.That.Matches(x => x.ODataQuery == "$filter=data/id/iv eq '123'")))
                .Returns(ResultList.CreateFrom<IEnrichedContentEntity>(0));

            await sut.HandleAsync(actor, context, entity1.Id, entity1);
            await sut.HandleAsync(actor, context, entity2.Id, entity2);

            A.CallTo(() => appProvider.GetSchemaAsync(context.App.Id, entity1.Schema))
                .MustHaveHappenedOnceExactly();
        }

        private IEnrichedContentEntity CreateContent(Guid id)
        {
            var content = A.Fake<IEnrichedContentEntity>();

            A.CallTo(() => content.Id).Returns(id);

            return content;
        }

        private ISchemaEntity CreateSchema(Guid id, string name)
        {
            var schema = A.Fake<ISchemaEntity>();

            A.CallTo(() => schema.Id).Returns(id);
            A.CallTo(() => schema.SchemaDef).Returns(new Schema(name));

            return schema;
        }
    }
}
