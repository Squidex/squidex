// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Scripting;

[Flags]
public enum ScriptScope
{
    None = 0,
    AssetScript = 1,
    AssetTrigger = 2,
    Async = 4,
    CommentTrigger = 8,
    ContentScript = 16,
    ContentTrigger = 32,
    SchemaTrigger = 128,
    Transform = 256,
    UsageTrigger = 512
}
