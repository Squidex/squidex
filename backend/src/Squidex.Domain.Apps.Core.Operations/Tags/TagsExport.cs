// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Tags;

public class TagsExport
{
    public Dictionary<string, Tag> Tags { get; set; } = new Dictionary<string, Tag>();

    public Dictionary<string, string> Alias { get; set; } = new Dictionary<string, string>();
}
