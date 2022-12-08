// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.ObjectModel;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Scripting;

public sealed class AssetEntityScriptVars : ScriptVars
{
    [FieldDescription(nameof(FieldDescriptions.AssetParentId))]
    public DomainId ParentId
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.AssetFileHash))]
    public string? FileHash
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.AssetFileName))]
    public string? FileName
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.AssetSlug))]
    public string? FileSlug
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.AssetMimeType))]
    public string? MimeType
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.AssetParentPath))]
    public Array? ParentPath
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.AssetMetadata))]
    public AssetMetadata? Metadata
    {
        set => SetValue(value != null ? new ReadOnlyDictionary<string, JsonValue>(value) : null);
    }

    [FieldDescription(nameof(FieldDescriptions.AssetTags))]
    public HashSet<string>? Tags
    {
        set => SetValue(value != null ? new ReadOnlyCollection<string>(value.ToList()) : null);
    }

    [FieldDescription(nameof(FieldDescriptions.AssetFileSize))]
    public long FileSize
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.AssetFileVersion))]
    public long FileVersion
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.AssetIsProtected))]
    public bool? IsProtected
    {
        set => SetValue(value);
    }
}
