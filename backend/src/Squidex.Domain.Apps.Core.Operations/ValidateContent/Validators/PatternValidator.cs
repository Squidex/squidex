// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.RegularExpressions;
using Squidex.Caching;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators;

public class PatternValidator : IValidator
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMilliseconds(20);

    // The validator tree is rebuilt for every content write, but parsing the pattern is by far the
    // most expensive part of it and only depends on the key, so it is shared over all validators.
    // Regex is thread safe for matching, therefore a single instance can be used concurrently.
    private static readonly LRUCache<(string Pattern, RegexOptions Options), Regex> Regexes = new LRUCache<(string, RegexOptions), Regex>(1000);

    // LRUCache is not thread safe and even TryGetValue mutates the recency list, so there is no
    // lock free read path and every access to the cache has to be guarded.
    private static readonly Lock RegexesLock = new Lock();

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

        regex = GetRegex(pattern, options);
    }

    private static Regex GetRegex(string pattern, RegexOptions options)
    {
        var cacheKey = (pattern, options);

        lock (RegexesLock)
        {
            if (Regexes.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }
        }

        // Parsing is the expensive part and must not be serialized over all threads, so it happens
        // outside the lock. Two threads can build the same pattern at the same time, which only
        // wastes a little work, and an invalid pattern still throws from here as it did before.
        var createdRegex = new Regex($"^{pattern}$", options, Timeout);

        lock (RegexesLock)
        {
            Regexes.Set(cacheKey, createdRegex);
        }

        return createdRegex;
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
