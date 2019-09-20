using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.ICIS.Validation.Validators;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Validation;
using Xunit;

#pragma warning disable IDE0060 // Remove unused parameter

namespace Squidex.ICIS.Test.Validation.Validators
{
    public class CommentaryPeriodValidatorTests
    {
        private readonly Context context = Context.Anonymous();
        private readonly CommentaryPeriodValidator sut;
        private readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
        private readonly IContextProvider contextProvider = A.Fake<IContextProvider>();
        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
        private readonly Guid contentId = Guid.NewGuid();
        private readonly Guid commentaryTypeId = Guid.NewGuid();

        public CommentaryPeriodValidatorTests()
        {
            sut = new CommentaryPeriodValidator(contentQuery);

            A.CallTo(() => contextProvider.Context).Returns(context);
        }

        public static object[][] InvalidPeriods()
        {
            return new object[][]
            {
                new object[] { JsonValue.Null },
                new object[] { JsonValue.True },
                new object[] { JsonValue.Array() },
                new object[] { null }
            };
        }

        [Theory]
        [MemberData(nameof(InvalidPeriods))]
        public async Task Should_add_error_if_period_required_and_no_period_defined(IJsonValue periodValue)
        {
            var commentaryType = CreateCommentaryType(JsonValue.True);

            A.CallTo(() => contentQuery.FindContentAsync(context, "commentary-type", commentaryTypeId, -2))
                .Returns(commentaryType);

            var errors = await sut.ValidateCommentaryAsync(contentId, schemaId, context, CreateCommentaryData(periodValue));

            errors.Should().BeEquivalentTo(new List<ValidationError>
            {
                new ValidationError("Period is required.")
            });
        }

        [Theory]
        [MemberData(nameof(InvalidPeriods))]
        public async Task Should_not_add_error_if_period_not_required_and_no_period_defined(IJsonValue periodValue)
        {
            var commentaryType = CreateCommentaryType(JsonValue.False);

            A.CallTo(() => contentQuery.FindContentAsync(context, "commentary-type", commentaryTypeId, -2))
                .Returns(commentaryType);

            var errors = await sut.ValidateCommentaryAsync(contentId, schemaId, context, CreateCommentaryData(periodValue));

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_period_required_and_period_defined()
        {
            var commentaryType = CreateCommentaryType(JsonValue.True);

            A.CallTo(() => contentQuery.FindContentAsync(context, "commentary-type", commentaryTypeId, -2))
                .Returns(commentaryType);

            var errors = await sut.ValidateCommentaryAsync(contentId, schemaId, context, CreateCommentaryData(JsonValue.Array("Id")));

            Assert.Empty(errors);
        }

        [Fact]
        public async Task Should_not_add_error_if_commentary_type_not_found()
        {
            var commentaryType = CreateCommentaryType(JsonValue.True);

            A.CallTo(() => contentQuery.FindContentAsync(context, "commentary-type", commentaryTypeId, -2))
                .Returns(Task.FromResult<IEnrichedContentEntity>(null));

            var errors = await sut.ValidateCommentaryAsync(contentId, schemaId, context, CreateCommentaryData(JsonValue.Array("Id")));

            Assert.Empty(errors);
        }

        private NamedContentData CreateCommentaryData(IJsonValue period)
        {
            var data = new NamedContentData()
                .AddField("commentarytype",
                    new ContentFieldData()
                        .AddValue(JsonValue.Array(commentaryTypeId.ToString())));

            if (period != null)
            {
                data.AddField("period",
                    new ContentFieldData()
                        .AddValue(period));
            }

            return data;
        }

        private IEnrichedContentEntity CreateCommentaryType(IJsonValue requiresPeriod)
        {
            var content = new ContentEntity
            {
                Data = new NamedContentData()
            };

            if (requiresPeriod != null)
            {
                content.Data.AddField("requires-period",
                    new ContentFieldData()
                        .AddValue(requiresPeriod));
            }

            return content;
        }
    }
}