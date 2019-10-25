// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Diagnostics.CodeAnalysis;

namespace Squidex.Infrastructure.EventSourcing
{
    public static class StreamFilter
    {
        public static bool IsAll([NotNullWhen(false)] string? filter)
        {
            return string.IsNullOrWhiteSpace(filter)
                || string.Equals(filter, ".*", StringComparison.OrdinalIgnoreCase)
                || string.Equals(filter, "(.*)", StringComparison.OrdinalIgnoreCase)
                || string.Equals(filter, "(.*?)", StringComparison.OrdinalIgnoreCase);
        }
    }
}
