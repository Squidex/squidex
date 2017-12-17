// ==========================================================================
//  AddPattern.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Entities.Apps.Commands
{
    public sealed class AddPattern : AppAggregateCommand
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Pattern { get; set; }

        public string DefaultMessage { get; set; }

        public AddPattern()
        {
            Id = Guid.NewGuid();
        }
    }
}
