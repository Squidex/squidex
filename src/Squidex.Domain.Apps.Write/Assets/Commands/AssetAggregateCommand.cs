// ==========================================================================
//  AssetAggregateCommand.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure.CQRS.Commands;

namespace Squidex.Domain.Apps.Write.Assets.Commands
{
    public abstract class AssetAggregateCommand : AppCommand, IAggregateCommand
    {
        public Guid AssetId { get; set; }

        Guid IAggregateCommand.AggregateId
        {
            get { return AssetId; }
        }
    }
}
