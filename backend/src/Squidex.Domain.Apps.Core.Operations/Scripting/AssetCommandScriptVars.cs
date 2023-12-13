// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Scripting;

public sealed class AssetCommandScriptVars : ScriptVars
{
    [FieldDescription(nameof(FieldDescriptions.AssetParentId))]
    public DomainId ParentId
    {
        set => SetInitial(value);
    }

    [FieldDescription(nameof(FieldDescriptions.AssetFileHash))]
    public string? FileHash
    {
        set => SetInitial(value);
    }

    [FieldDescription(nameof(FieldDescriptions.AssetFileName))]
    public string? FileName
    {
        set => SetInitial(value);
    }

    [FieldDescription(nameof(FieldDescriptions.AssetSlug))]
    public string? FileSlug
    {
        set => SetInitial(value);
    }

    [FieldDescription(nameof(FieldDescriptions.AssetMimeType))]
    public string? MimeType
    {
        set => SetInitial(value);
    }

    [FieldDescription(nameof(FieldDescriptions.AssetParentPath))]
    public Array? ParentPath
    {
        set => SetInitial(value);
    }

    [FieldDescription(nameof(FieldDescriptions.AssetMetadata))]
    public AssetMetadata? Metadata
    {
        set => SetInitial(value);
    }

    [FieldDescription(nameof(FieldDescriptions.AssetTags))]
    public HashSet<string>? Tags
    {
        set => SetInitial(value);
    }

    [FieldDescription(nameof(FieldDescriptions.AssetFileSize))]
    public long FileSize
    {
        set => SetInitial(value);
    }

    [FieldDescription(nameof(FieldDescriptions.AssetIsProtected))]
    public bool? IsProtected
    {
        set => SetInitial(value);
    }

    [FieldDescription(nameof(FieldDescriptions.EntityRequestDeletePermanent))]
    public bool? Permanent
    {
        set => SetInitial(value);
    }
}
