using System;
using System.Collections.Generic;
using FakeItEasy;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.ICIS.Test.TestHelpers;
using Squidex.ICIS.Validation.Validators;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.ICIS.Test.Validation.Validators
{
    public class CommentaryCharacterCountValidatorTests
    {
        private readonly Context context = new Context();
        private readonly TestHelper testHelper;
        private readonly CommentaryCharacterCountValidator commentaryCharacterCountValidator;

        private readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
        private readonly IContextProvider contextProvider = A.Fake<IContextProvider>();

        private readonly Guid regionGuid = Guid.NewGuid();
        private readonly Guid commodityGuid = Guid.NewGuid();
        private readonly Guid commentaryTypeGuid = Guid.NewGuid();

        public CommentaryCharacterCountValidatorTests()
        {
            testHelper = new TestHelper();
            commentaryCharacterCountValidator = new CommentaryCharacterCountValidator(contentQuery);

            A.CallTo(() => contextProvider.Context).Returns(context);
        }

        [Theory]
        [InlineData("All commentary bodies < character limits", new []{"ab", "ab"}, new []{3, 3})]
        [InlineData("All commentary bodies == character limits", new[] { "abc", "abc" }, new[] { 3, 3 })]
        [InlineData("All commentary bodies <= character limits", new[] { "ab", "abc" }, new[] { 3, 3 })]
        public async void Should_pass_if_all_commentary_body_validation_requirements_are_met(string testSummary, string[] bodyStringInputs, int[] characterLimitInputs)
        {
            var body = new ContentFieldData().AddValue("en", bodyStringInputs[0]).AddValue("zh", bodyStringInputs[1]);
            var commentary = CreateWorkingCommmentaryContent(body);

            var characterLimit = new ContentFieldData().AddValue("en", characterLimitInputs[0]).AddValue("zh", characterLimitInputs[1]);
            var commentaryType = CreateWorkingCommentaryTypeContent(characterLimit);

            var commentaryTypeEntity = testHelper.CreateEnrichedContent(commentaryType.ContentId, new Guid(), new Guid(), commentaryType.Data);

            A.CallTo(() => contentQuery.FindContentAsync(context, commentaryType.SchemaId.Name, commentaryType.ContentId, A<long>.Ignored))
                .Returns(commentaryTypeEntity);

            var errors = await commentaryCharacterCountValidator.ValidateCommentaryAsync(commentary.ContentId, commentary.SchemaId, context, commentary.Data);

            errors.Should().BeEmpty();
        }

        [Fact]
        public async void Should_fail_with_validation_error_if_single_language_commentary_body_character_count_exceeded()
        {
            var body = new ContentFieldData().AddValue("en", "abcd").AddValue("zh", "abc");
            var commentary = CreateWorkingCommmentaryContent(body);

            var characterLimit = new ContentFieldData().AddValue("en", 3).AddValue("zh", 3);
            var commentaryType = CreateWorkingCommentaryTypeContent(characterLimit);

            var commentaryTypeEntity = testHelper.CreateEnrichedContent(
                commentaryType.ContentId, new Guid(), new Guid(), commentaryType.Data);

            A.CallTo(() => contentQuery.FindContentAsync(context, commentaryType.SchemaId.Name, commentaryType.ContentId, A<long>.Ignored))
                .Returns(commentaryTypeEntity);

            var errors = await commentaryCharacterCountValidator.ValidateCommentaryAsync(commentary.ContentId, commentary.SchemaId, context, commentary.Data);

            errors.Should().BeEquivalentTo(new List<ValidationError>
            {
                new ValidationError("Exceeded character limit of '3' characters for language 'en'")
            });
        }

        [Fact]
        public async void Should_give_multiple_validation_errors_if_multi_language_commentary_body_character_count_exceeded()
        {
            var body = new ContentFieldData().AddValue("en", "abcd").AddValue("zh", "abcd");
            var commentary = CreateWorkingCommmentaryContent(body);

            var characterLimit = new ContentFieldData().AddValue("en", 3).AddValue("zh", 3);
            var commentaryType = CreateWorkingCommentaryTypeContent(characterLimit);

            var commentaryTypeEntity = testHelper.CreateEnrichedContent(
                commentaryType.ContentId, new Guid(), new Guid(), commentaryType.Data);

            A.CallTo(() => contentQuery.FindContentAsync(context, commentaryType.SchemaId.Name, commentaryType.ContentId, A<long>.Ignored))
                .Returns(commentaryTypeEntity);

            var errors = await commentaryCharacterCountValidator.ValidateCommentaryAsync(commentary.ContentId, commentary.SchemaId, context, commentary.Data);

            errors.Should().BeEquivalentTo(new List<ValidationError>
            {
                new ValidationError("Exceeded character limit of '3' characters for language 'en'"),
                new ValidationError("Exceeded character limit of '3' characters for language 'zh'")
            });
        }

        [Theory]
        [InlineData("Bullet-Points & Bold Words", "* **Hello** \n* **World**", 10)]
        [InlineData("Bullet-Points & Italic Words", "* *Hello* \n* *World*",10)]
        [InlineData("Bullet-Points, Bold & Italic Words", "* ***Hello***\n* ***World***", 10)]
        public async void Should_ignore_markdown_characters_in_validation(string summary, string bodyStringInput, int characterLimitInput)
        {
            var body = new ContentFieldData().AddValue("en", bodyStringInput);
            var commentary = CreateWorkingCommmentaryContent(body);

            var characterLimit = new ContentFieldData().AddValue("en", characterLimitInput);
            var commentaryType = CreateWorkingCommentaryTypeContent(characterLimit);

            var commentaryTypeEntity = testHelper.CreateEnrichedContent(commentaryType.ContentId, new Guid(), new Guid(), commentaryType.Data);

            A.CallTo(() => contentQuery.FindContentAsync(context, commentaryType.SchemaId.Name, commentaryType.ContentId, A<long>.Ignored))
                .Returns(commentaryTypeEntity);

            var errors = await commentaryCharacterCountValidator.ValidateCommentaryAsync(commentary.ContentId, commentary.SchemaId, context, commentary.Data);

            errors.Should().BeEmpty();
        }

        [Fact]
        public async void Only_validate_commentary_with_character_limit_set()
        {
            var body = new ContentFieldData().AddValue("en", "abcd").AddValue("zh", "abcd");
            var commentary = CreateWorkingCommmentaryContent(body);

            var characterLimit = new ContentFieldData().AddValue("en", 3);
            var commentaryType = CreateWorkingCommentaryTypeContent(characterLimit);

            var commentaryTypeEntity = testHelper.CreateEnrichedContent(commentaryType.ContentId, new Guid(), new Guid(), commentaryType.Data);

            A.CallTo(() => contentQuery.FindContentAsync(contextProvider.Context, commentaryType.SchemaId.Name, commentaryType.ContentId, A<long>.Ignored))
                .Returns(commentaryTypeEntity);

            var errors = await commentaryCharacterCountValidator.ValidateCommentaryAsync(commentary.ContentId, commentary.SchemaId, context, commentary.Data);

            errors.Should().BeEquivalentTo(new List<ValidationError>
            {
                new ValidationError("Exceeded character limit of '3' characters for language 'en'"),
            });
        }

        private CreateContent CreateWorkingCommmentaryContent(ContentFieldData body)
        {
            var content = new CreateContent();

            content.SchemaId = new NamedId<Guid>(Guid.NewGuid(), "commentary");

            var currentTime = NodaTime.Instant.FromDateTimeUtc(DateTime.Now.ToUniversalTime());

            var data = new NamedContentData {
                {"createdfor", new ContentFieldData().AddJsonValue("iv", JsonValue.Create(currentTime))},
                {"region", new ContentFieldData().AddJsonValue("iv", new JsonArray {JsonValue.Create(regionGuid.ToString())})},
                {"commodity", new ContentFieldData().AddJsonValue("iv",new JsonArray {JsonValue.Create(commodityGuid.ToString())})},
                {"commentarytype", new ContentFieldData().AddJsonValue("iv",new JsonArray {JsonValue.Create(commentaryTypeGuid.ToString())})},
                {"body", body}
            };

            content.Data = data;

            return content;
        }

        private CreateContent CreateWorkingCommentaryTypeContent(ContentFieldData characterLimit)
        {
            var content = new CreateContent();

            content.SchemaId = new NamedId<Guid>(Guid.NewGuid(), "commentary-type");
            content.ContentId = commentaryTypeGuid;

            var data = new NamedContentData()
            {
                {"id", new ContentFieldData().AddJsonValue("iv", JsonValue.Create("testId")) },
                {"name", new ContentFieldData().AddValue("iv", JsonValue.Create("testName")) },
                {"character-limit", characterLimit }
            };

            content.Data = data;

            return content;
        }

    }
}