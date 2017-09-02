// ==========================================================================
//  ScriptContext.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public sealed class ScriptContext
    {
        public ScriptUser User { get; set; }

        public Guid ContentId { get; set; }

        public NamedContentData Data { get; set; }

        public NamedContentData OldData { get; set; }
    }
}
