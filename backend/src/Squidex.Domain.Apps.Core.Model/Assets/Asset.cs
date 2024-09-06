// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.Contracts;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Assets;

public record Asset : AssetItem
{
    public string FileName { get; set; }

    public string FileHash { get; set; }

    public string MimeType { get; set; }

    public string Slug { get; set; }

    public long FileSize { get; set; }

    public long FileVersion { get; set; }

    public long TotalSize { get; set; }

    public bool IsProtected { get; set; }

    public HashSet<string> Tags { get; set; } = [];

    public AssetMetadata Metadata { get; set; } = [];

    public AssetType Type { get; set; }

    [Pure]
    public Asset Move(DomainId parentId)
    {
        if (Equals(ParentId, parentId))
        {
            return this;
        }

        return this with { ParentId = parentId };
    }

    [Pure]
    public Asset Annotate(string? fileName = null, string? slug = null, bool? isProtected = null,
        HashSet<string>? tags = null, AssetMetadata? metadata = null)
    {
        var result = this;

        if (fileName != null && !string.Equals(FileName, fileName, StringComparison.OrdinalIgnoreCase))
        {
            result = result with { FileName = fileName };
        }

        if (slug != null && !string.Equals(Slug, slug, StringComparison.OrdinalIgnoreCase))
        {
            result = result with { Slug = slug };
        }

        if (isProtected != null && IsProtected != isProtected.Value)
        {
            result = result with { IsProtected = isProtected.Value };
        }

        if (tags != null && !Tags.SetEquals(tags))
        {
            result = result with { Tags = tags };
        }

        if (metadata != null && !Metadata.EqualsDictionary(metadata))
        {
            result = result with { Metadata = metadata };
        }

        return result;
    }
}
