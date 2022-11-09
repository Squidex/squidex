// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Scripting;

public sealed class AssetScriptVars : ScriptVars
{
    [FieldDescription(nameof(FieldDescriptions.AppId))]
    public DomainId AppId
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.EntityId))]
    public DomainId AssetId
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.AppName))]
    public string AppName
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.Operation))]
    public string Operation
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.Command))]
    public AssetCommandScriptVars Command
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.Asset))]
    public AssetEntityScriptVars Asset
    {
        set => SetValue(value);
    }

    [FieldDescription(nameof(FieldDescriptions.User))]
    public ClaimsPrincipal? User
    {
        set => SetValue(value);
    }
}
