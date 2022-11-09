// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Tags;

public sealed class TagsSet : Dictionary<string, int>
{
    public long Version { get; set; }

    public TagsSet()
    {
    }

    public TagsSet(IDictionary<string, int> tags, long version)
        : base(tags)
    {
        Version = version;
    }
}
