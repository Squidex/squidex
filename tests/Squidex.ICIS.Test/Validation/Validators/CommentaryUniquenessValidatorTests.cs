using System;
using FakeItEasy;
using System.Collections.Generic;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.ICIS.Validation;
using Squidex.ICIS.Test.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.ICIS.Test.Validation.Validators
{
    public class CommentaryUniquenessValidatorTests
    {
        private readonly Context context = new Context();
        private readonly TestHelper testHelper;
        private readonly CommentaryUniquenessValidator commentaryUniquenessValidator;

        private readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
        private readonly IContextProvider contextProvider = A.Fake<IContextProvider>();

        private readonly Guid regionGuid = Guid.NewGuid();
        private readonly Guid commodityGuid = Guid.NewGuid();
        private readonly Guid commentaryTypeGuid = Guid.NewGuid();

        public CommentaryUniquenessValidatorTests()
        {
            testHelper = new TestHelper();
            commentaryUniquenessValidator = new CommentaryUniquenessValidator(contentQuery);

            A.CallTo(() => contextProvider.Context).Returns(context);
        }

        [Fact]
        public async void Should_reject_if_not_all_fields_present()
        {
            var commentary = CreateBrokenCommentary();

            await Assert.ThrowsAsync<NullReferenceException>(() => commentaryUniquenessValidator.ValidateCommentaryAsync(commentary.ContentId,
                commentary.SchemaId, context, commentary.Data));
        }

        [Fact]
        public async void Should_fail_if_existing_document_is_found()
        {
            var commentary = CreateWorkingCommentary();
            var contentEntity = testHelper.CreateEnrichedContent(new Guid(), new Guid(), new Guid()); 

            A.CallTo(() => contentQuery.QueryAsync(context, commentary.SchemaId.Name, A<Q>.Ignored))
                .Returns(ResultList.CreateFrom(1, contentEntity));

            var errors = await commentaryUniquenessValidator.ValidateCommentaryAsync(commentary.ContentId,
                commentary.SchemaId, context, commentary.Data);

            errors.Should().BeEquivalentTo(new List<ValidationError>
            {
                new ValidationError("A content item with these values already exists.")
            });
        }

        [Fact]
        public async void Should_pass_if_all_fields_present_and_unique()
        {
            var commentary = CreateWorkingCommentary();

            var errors = await commentaryUniquenessValidator.ValidateCommentaryAsync(
                commentary.ContentId, commentary.SchemaId, context, commentary.Data);

            errors.Should().BeEmpty();
        }

        [Fact]
        public async void Should_pass_if_document_from_db_has_same_id()
        {
            var commentary = CreateWorkingCommentary();
            var contentEntity = testHelper.CreateEnrichedContent(commentary.ContentId, new Guid(), new Guid());

            A.CallTo(() => contentQuery.QueryAsync(context, commentary.SchemaId.Name, A<Q>.Ignored))
                .Returns(ResultList.CreateFrom(1, contentEntity));

            var errors = await commentaryUniquenessValidator.ValidateCommentaryAsync(commentary.ContentId,
                commentary.SchemaId, context, commentary.Data);

            errors.Should().BeEmpty();
        }

        private CreateContent CreateBrokenCommentary()
        {
            var commentary = new CreateContent();

            var data = new NamedContentData {
                {"region", null},
                {"commodity", new ContentFieldData()},
                {"commentarytype", new ContentFieldData()},
                {"createdfor", new ContentFieldData()},
                {"iv", new ContentFieldData()}
            };             

            commentary.Data = data;

            return commentary;
        }

        private CreateContent CreateWorkingCommentary()
        {
            var commentary = new CreateContent();

            commentary.SchemaId = new NamedId<Guid>(Guid.NewGuid(), "test");

            var currentTime = NodaTime.Instant.FromDateTimeUtc(DateTime.Now.ToUniversalTime());

            var data = new NamedContentData {
                {"createdfor", new ContentFieldData().AddJsonValue("iv", JsonValue.Create(currentTime))},
                {"region", new ContentFieldData().AddJsonValue("iv", new JsonArray {JsonValue.Create(regionGuid.ToString())})},
                {"commodity", new ContentFieldData().AddJsonValue("iv",new JsonArray {JsonValue.Create(commodityGuid.ToString())})},
                {"commentarytype", new ContentFieldData().AddJsonValue("iv",new JsonArray {JsonValue.Create(commentaryTypeGuid.ToString())})}
            };

            commentary.Data = data;

            return commentary;
        }

    }
}
