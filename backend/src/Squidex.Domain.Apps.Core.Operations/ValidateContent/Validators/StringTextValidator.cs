// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;
using Squidex.Text;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators;

public sealed class StringTextValidator : IValidator
{
    private readonly Func<string, string>? transform;
    private readonly int? minCharacters;
    private readonly int? maxCharacters;
    private readonly int? minWords;
    private readonly int? maxWords;

    public StringTextValidator(Func<string, string>? transform = null,
        int? minCharacters = null,
        int? maxCharacters = null,
        int? minWords = null,
        int? maxWords = null)
    {
        if (minCharacters > maxCharacters)
        {
            ThrowHelper.ArgumentException("Min characters must be greater than max characters.", nameof(minCharacters));
        }

        if (minWords > maxWords)
        {
            ThrowHelper.ArgumentException("Min words must be greater than max words.", nameof(minWords));
        }

        this.transform = transform;
        this.minCharacters = minCharacters;
        this.maxCharacters = maxCharacters;
        this.minWords = minWords;
        this.maxWords = maxWords;
    }

    public void Validate(object? value, ValidationContext context)
    {
        if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
        {
            if (transform != null)
            {
                stringValue = transform(stringValue);
            }

            if (minWords != null || maxWords != null)
            {
                var words = stringValue.WordCount();

                if (minWords != null && maxWords != null)
                {
                    if (minWords == maxWords && minWords != words)
                    {
                        context.AddError(context.Path, T.Get("contents.validation.wordCount", new { count = minWords }));
                    }
                    else if (words < minWords || words > maxWords)
                    {
                        context.AddError(context.Path, T.Get("contents.validation.wordsBetween", new { min = minWords, max = maxWords }));
                    }
                }
                else
                {
                    if (words < minWords)
                    {
                        context.AddError(context.Path, T.Get("contents.validation.minWords", new { min = minWords }));
                    }

                    if (words > maxWords)
                    {
                        context.AddError(context.Path, T.Get("contents.validation.maxWords", new { max = maxWords }));
                    }
                }
            }

            if (minCharacters != null || maxCharacters != null)
            {
                var characters = stringValue.CharacterCount();

                if (minCharacters != null && maxCharacters != null)
                {
                    if (minCharacters == maxCharacters && minCharacters != characters)
                    {
                        context.AddError(context.Path, T.Get("contents.validation.normalCharacterCount", new { count = minCharacters }));
                    }
                    else if (characters < minCharacters || characters > maxCharacters)
                    {
                        context.AddError(context.Path, T.Get("contents.validation.normalCharactersBetween", new { min = minCharacters, max = maxCharacters }));
                    }
                }
                else
                {
                    if (characters < minCharacters)
                    {
                        context.AddError(context.Path, T.Get("contents.validation.minNormalCharacters", new { min = minCharacters }));
                    }

                    if (characters > maxCharacters)
                    {
                        context.AddError(context.Path, T.Get("contents.validation.maxCharacters", new { max = maxCharacters }));
                    }
                }
            }
        }
    }
}
