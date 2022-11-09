// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Core.Scripting;

public class DataScriptVars : ScriptVars
{
    public virtual ContentData? Data
    {
        get => GetValue<ContentData?>();
        set => SetValue(value);
    }
}
