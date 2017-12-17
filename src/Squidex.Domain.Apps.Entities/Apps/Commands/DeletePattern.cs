// ==========================================================================
//  DeletePattern.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Entities.Apps.Commands
{
    public sealed class DeletePattern : AppAggregateCommand
    {
        public Guid PatternId { get; set; }
    }
}
