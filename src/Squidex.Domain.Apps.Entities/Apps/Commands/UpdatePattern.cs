// ==========================================================================
//  UpdatePattern.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Entities.Apps.Commands
{
    public sealed class UpdatePattern : AppAggregateCommand
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Pattern { get; set; }

        public string Message { get; set; }
    }
}
