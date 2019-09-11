using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Markdig;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.ICIS.Utilities;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.ICIS.Validation.Validators
{
    public class CommentaryCharacterCountValidator : ICommentaryValidator
    {
        private readonly IContentQueryService contentQuery;

        public CommentaryCharacterCountValidator(IContentQueryService contentQuery)
        {
            this.contentQuery = contentQuery;
        }

        public async Task<IEnumerable<ValidationError>> ValidateCommentaryAsync(Guid contentId, NamedId<Guid> schemaId, Context context, NamedContentData data)
        {
            if (data.TryGetGuid("commentarytype", out var commentaryTypeId))
            {
                var validationContent = await contentQuery.FindContentAsync(context, "commentary-type", commentaryTypeId);

                return ValidateContentFieldCharacterCount(validationContent, "character-limit", data, "body").ToList();
            }

            return null;
        }

        private IEnumerable<ValidationError> ValidateContentFieldCharacterCount(IEnrichedContentEntity validationContent,
            string validationField, NamedContentData content, string contentField)
        {
            if (content.TryGetValue(contentField, out var body) &&
                body != null &&
                validationContent != null &&
                validationContent.Data.TryGetValue(validationField, out var characterLimits))
            {
                foreach (var language in body)
                {
                    if (language.Value is JsonString s)
                    {
                        if (characterLimits.TryGetNumber(out var characterLimit, language.Key) || characterLimits.TryGetNumber(out characterLimit))
                        {
                            var plainTextBody = Regex.Replace(Markdown.ToPlainText(s.Value), "\n", "");
                            var characterCount = plainTextBody.Length;

                            if (characterCount > characterLimit)
                            {
                                yield return new ValidationError($"Exceeded character limit of '{characterLimit}' characters for language '{language.Key}'");
                            }
                        }
                    }
                }
            }
        }
    }
}