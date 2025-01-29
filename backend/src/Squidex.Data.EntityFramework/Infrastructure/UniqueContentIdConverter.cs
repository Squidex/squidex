// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Contents.Text;

namespace Squidex.Infrastructure;

public static class UniqueContentIdConverter
{
    public static string ToParseableString(this UniqueContentId source)
    {
        return $"{source.AppId}__{source.ContentId}";
    }

    public static UniqueContentId ToUniqueContentId(this string source)
    {
        var separator = source.IndexOf("__", StringComparison.Ordinal);
        if (separator < 0)
        {
            throw new ArgumentException("Invalid ID", nameof(source));
        }

        return new UniqueContentId(
            DomainId.Create(source[..separator]),
            DomainId.Create(source[(separator + 2)..]));
    }
}
