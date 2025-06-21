// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators;

public class StringLengthValidator : IValidator
{
    private readonly int? minLength;
    private readonly int? maxLength;

    public StringLengthValidator(int? minLength = null, int? maxLength = null)
    {
        if (minLength > maxLength)
        {
            ThrowHelper.ArgumentException("Min length must be greater than max length.", nameof(minLength));
        }

        this.minLength = minLength;
        this.maxLength = maxLength;
    }

    public void Validate(object? value, ValidationContext context)
    {
        if (value is not string stringValue || string.IsNullOrEmpty(stringValue))
        {
            return;
        }

        if (minLength != null && maxLength != null)
        {
            if (minLength == maxLength && minLength != stringValue.Length)
            {
                context.AddError(T.Get("contents.validation.characterCount", new { count = minLength }), context.Path);
            }
            else if (stringValue.Length < minLength || stringValue.Length > maxLength)
            {
                context.AddError(T.Get("contents.validation.charactersBetween", new { min = minLength, max = maxLength }), context.Path);
            }
        }
        else
        {
            if (minLength != null && stringValue.Length < minLength)
            {
                context.AddError(T.Get("contents.validation.minLength", new { min = minLength }), context.Path);
            }

            if (maxLength != null && stringValue.Length > maxLength)
            {
                context.AddError(T.Get("contents.validation.maxLength", new { max = maxLength }), context.Path);
            }
        }
    }
}
