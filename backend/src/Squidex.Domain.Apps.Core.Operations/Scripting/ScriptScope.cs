// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Scripting
{
    [Flags]
    public enum ScriptScope
    {
        Async,
        AssetScript,
        AssetTrigger,
        ContentScript,
        ContentTrigger,
        Transform,
        SchemaTrigger,
        UsageTrigger,
        CommentTrigger
    }
}
