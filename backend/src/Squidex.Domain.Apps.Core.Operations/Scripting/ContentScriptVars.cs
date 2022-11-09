// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Scripting;

public sealed class ContentScriptVars : DataScriptVars
{
    [FieldDescription(nameof(FieldDescriptions.ContentValidate))]
    public Action Validate
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.AppId))]
    public DomainId AppId
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.SchemaId))]
    public DomainId SchemaId
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.EntityId))]
    public DomainId ContentId
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.AppName))]
    public string AppName
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.ContentSchemaName))]
    public string SchemaName
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.Operation))]
    public string Operation
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.EntityRequestDeletePermanent))]
    public bool Permanent
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.User))]
    public ClaimsPrincipal? User
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.ContentStatus))]
    public Status Status
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.ContentStatusOld))]
    public Status StatusOld
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.ContentStatusOld))]
    public Status OldStatus
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.ContentData))]
    public ContentData? DataOld
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.ContentDataOld))]
    public ContentData? OldData
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.ContentData))]
    public override ContentData? Data
    {
        get => GetValue<ContentData?>();
        set => SetValue(value);
    }
}
