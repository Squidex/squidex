﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators;

public class PatternValidator : IValidator
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(20);
    private readonly Regex regex;
    private readonly string? errorMessage;

    public PatternValidator(string pattern, string? errorMessage = null, bool capture = false)
    {
        Guard.NotNullOrEmpty(pattern);

        this.errorMessage = errorMessage;

        var options = RegexOptions.None;

        if (!capture)
        {
            options |= RegexOptions.ExplicitCapture;
        }

        regex = new Regex($"^{pattern}$", options, Timeout);
    }

    public void Validate(object? value, ValidationContext context)
    {
        if (value is not string stringValue || string.IsNullOrEmpty(stringValue))
        {
            return;
        }

        try
        {
            if (!regex.IsMatch(stringValue))
            {
                if (string.IsNullOrWhiteSpace(errorMessage))
                {
                    context.AddError(T.Get("contents.validation.pattern"));
                }
                else
                {
                    context.AddError(errorMessage);
                }
            }
        }
        catch
        {
            context.AddError(T.Get("contents.validation.regexTooSlow"));
        }
    }
}
