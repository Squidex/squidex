// ==========================================================================
//  ChangeContentStatus.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// =========================================================================

using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Write.Contents.Commands
{
    public sealed class ChangeContentStatus : ContentCommand
    {
        public Status Status { get; set; }
    }
}
