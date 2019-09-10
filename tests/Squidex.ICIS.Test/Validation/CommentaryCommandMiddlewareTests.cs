using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.ICIS.Validation;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Xunit;

namespace Squidex.ICIS.Test.Validation
{
    public class CommentaryCommandMiddlewareTests
    {
        private readonly Func<Task> next = A.Fake<Func<Task>>();
        private readonly Context context = new Context();

        private readonly IContextProvider contextProvider = A.Fake<IContextProvider>();
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly ICommandMiddleware commentaryCommandMiddleware;
        private readonly ICommentaryValidator commentaryValidator = A.Fake<ICommentaryValidator>();

        public CommentaryCommandMiddlewareTests()
        {
            var validators = new List<ICommentaryValidator>()
            {
                commentaryValidator,
                commentaryValidator
            };

            commentaryCommandMiddleware = new CommentaryCommandMiddleware(validators, contextProvider, grainFactory);

            A.CallTo(() => contextProvider.Context).Returns(context);
        }

        [Fact]
        public async void Should_call_all_commentary_validators()
        {
            var contentCommand = CreateWorkingCreateContentCommand();
            var commandContext = new CommandContext(contentCommand, new InMemoryCommandBus(new List<ICommandMiddleware>()));

            A.CallTo(() => commentaryValidator.ValidateCommentaryAsync(contentCommand.ContentId, contentCommand.SchemaId, context, contentCommand.Data))
                .Returns(new List<ValidationError>());

            await commentaryCommandMiddleware.HandleAsync(commandContext, next);

            A.CallTo(() => commentaryValidator.ValidateCommentaryAsync(contentCommand.ContentId, contentCommand.SchemaId, context, contentCommand.Data))
                .MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public void Should_return_any_validation_errors_on_creation_of_invalid_commentary()
        {
            var contentCommand = CreateWorkingCreateContentCommand();
            var commandContext = new CommandContext(contentCommand, new InMemoryCommandBus(new List<ICommandMiddleware>()));

            A.CallTo(() => commentaryValidator.ValidateCommentaryAsync(contentCommand.ContentId, contentCommand.SchemaId, context, contentCommand.Data))
                .Returns(new List<ValidationError>(){new ValidationError("Validation Message")});

            var ex = Assert.ThrowsAsync<ValidationException>(() => commentaryCommandMiddleware.HandleAsync(commandContext, next));
            Assert.Equal("Failed to save commentary: Validation Message. Validation Message.", ex.Result.Message);
        }

        [Fact]
        public void Should_return_any_validation_errors_on_updating_of_invalid_commentary()
        {
            var schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
            var contentCommand = CreateWorkingUpdateContentCommand();
            var commandContext = new CommandContext(contentCommand, new InMemoryCommandBus(new List<ICommandMiddleware>()));

            var contentGrain = A.Fake<IContentGrain>();

            A.CallTo(() => grainFactory.GetGrain<IContentGrain>(contentCommand.ContentId, null)).Returns(contentGrain);

            A.CallTo(() => contentGrain.GetStateAsync(A<long>.Ignored)).Returns(new ContentEntity { SchemaId = schemaId });

            A.CallTo(() => commentaryValidator.ValidateCommentaryAsync(contentCommand.ContentId, schemaId, context, contentCommand.Data))
                .Returns(new List<ValidationError>() { new ValidationError("Validation Message") });

            var ex = Assert.ThrowsAsync<ValidationException>(() => commentaryCommandMiddleware.HandleAsync(commandContext, next));
            Assert.Equal("Failed to save commentary: Validation Message. Validation Message.", ex.Result.Message);
        }

        public CreateContent CreateWorkingCreateContentCommand()
        {
            var contentCommand = new CreateContent();
            contentCommand.SchemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
            contentCommand.ContentId = Guid.NewGuid();
            contentCommand.Data = new NamedContentData();

            return contentCommand;
        }

        private UpdateContent CreateWorkingUpdateContentCommand()
        {
            var contentCommand = new UpdateContent();
            contentCommand.ContentId = Guid.NewGuid();
            contentCommand.Data = new NamedContentData();

            return contentCommand;
        }
    }
}