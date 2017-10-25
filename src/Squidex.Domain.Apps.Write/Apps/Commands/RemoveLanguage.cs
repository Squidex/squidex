// ==========================================================================
//  RemoveLanguage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Apps.Commands
{
    public sealed class RemoveLanguage : AppAggregateCommand
    {
        public Language Language { get; set; }
    }
}
