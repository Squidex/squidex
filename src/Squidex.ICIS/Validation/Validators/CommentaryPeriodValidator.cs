using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.ICIS.Utilities;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Validation;

namespace Squidex.ICIS.Validation.Validators
{
    public class CommentaryPeriodValidator : ICommentaryValidator
    {
        private readonly IContentQueryService contentQuery;

        public CommentaryPeriodValidator(IContentQueryService contentQuery)
        {
            this.contentQuery = contentQuery;
        }

        public async Task<IEnumerable<ValidationError>> ValidateCommentaryAsync(Guid contentId, NamedId<Guid> schemaId, Context context, NamedContentData data)
        {
            if (data.TryGetGuid("commentarytype", out var commentaryTypeId))
            {
                var commentaryType = await contentQuery.FindContentAsync(context.WithUnpublished(true), "commentary-type", commentaryTypeId);

                return ValidatePeriod(commentaryType, "requires-period", data, "period").ToList();
            }

            return null;
        }

        private IEnumerable<ValidationError> ValidatePeriod(IEnrichedContentEntity commentaryType,
            string validationField, NamedContentData content, string contentField)
        {
            var period = content.GetOrDefault(contentField) ?? new ContentFieldData();

            if (commentaryType != null &&
                commentaryType.Data.TryGetValue(validationField, out var requiredPeriod))
            {
                if (requiredPeriod.TryGetValue("iv", out var v) && v is JsonBoolean b && b.Value)
                {
                    var periodValue = period.GetOrDefault("iv");

                    if (!(periodValue is JsonArray array) || array.Count == 0)
                    {
                        yield return new ValidationError($"Period is required.");
                    }
                }
            }
        }
    }
}