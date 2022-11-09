// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;

namespace Squidex.Domain.Apps.Entities.Assets.Commands;

public sealed class AnnotateAsset : AssetCommand
{
    public string? FileName { get; set; }

    public string? Slug { get; set; }

    public bool? IsProtected { get; set; }

    public HashSet<string> Tags { get; set; }

    public AssetMetadata? Metadata { get; set; }
}
