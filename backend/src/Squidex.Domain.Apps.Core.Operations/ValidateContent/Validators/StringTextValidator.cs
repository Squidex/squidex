﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Translations;
using Squidex.Text;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
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
                throw new ArgumentException("Min characters must be greater than max characters.", nameof(minCharacters));
            }

            if (minWords > maxWords)
            {
                throw new ArgumentException("Min words must be greater than max words.", nameof(minWords));
            }

            this.transform = transform;
            this.minCharacters = minCharacters;
            this.maxCharacters = maxCharacters;
            this.minWords = minWords;
            this.maxWords = maxWords;
        }

        public Task ValidateAsync(object? value, ValidationContext context, AddError addError)
        {
            if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
            {
                if (transform != null)
                {
                    stringValue = transform(stringValue);
                }

                if (minWords.HasValue || maxWords.HasValue)
                {
                    var words = stringValue.WordCount();

                    if (minWords.HasValue && maxWords.HasValue)
                    {
                        if (minWords == maxWords && minWords != words)
                        {
                            addError(context.Path, T.Get("contents.validation.wordCount", new { count = minWords }));
                        }
                        else if (words < minWords || words > maxWords)
                        {
                            addError(context.Path, T.Get("contents.validation.wordsBetween", new { min = minWords, max = maxWords }));
                        }
                    }
                    else
                    {
                        if (minWords.HasValue && words < minWords)
                        {
                            addError(context.Path, T.Get("contents.validation.minWords", new { min = minWords }));
                        }

                        if (maxWords.HasValue && words > maxWords)
                        {
                            addError(context.Path, T.Get("contents.validation.maxWords", new { max = maxWords }));
                        }
                    }
                }

                if (minCharacters.HasValue || maxCharacters.HasValue)
                {
                    var characters = stringValue.CharacterCount();

                    if (minCharacters.HasValue && maxCharacters.HasValue)
                    {
                        if (minCharacters == maxCharacters && minCharacters != characters)
                        {
                            addError(context.Path, T.Get("contents.validation.normalCharacterCount", new { count = minCharacters }));
                        }
                        else if (characters < minCharacters || characters > maxCharacters)
                        {
                            addError(context.Path, T.Get("contents.validation.normalCharactersBetween", new { min = minCharacters, max = maxCharacters }));
                        }
                    }
                    else
                    {
                        if (minCharacters.HasValue && characters < minCharacters)
                        {
                            addError(context.Path, T.Get("contents.validation.minNormalCharacters", new { min = minCharacters }));
                        }

                        if (maxCharacters.HasValue && characters > maxCharacters)
                        {
                            addError(context.Path, T.Get("contents.validation.maxCharacters", new { max = maxCharacters }));
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
