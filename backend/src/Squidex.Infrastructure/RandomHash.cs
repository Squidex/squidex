// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure;

public static class RandomHash
{
    public static string New()
    {
        return Guid.NewGuid()
            .ToString()
            .ToSha256Base64()
            .ToLowerInvariant()
            .Replace('+', 'x')
            .Replace('=', 'x')
            .Replace('/', 'x');
    }

    public static string Simple()
    {
        return Guid.NewGuid().ToString().Replace("-", string.Empty, StringComparison.Ordinal);
    }
}
