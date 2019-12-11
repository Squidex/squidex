﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Assets.Commands
{
    public abstract class AssetFolderCommand : SquidexCommand, IAggregateCommand
    {
        public Guid AssetFolderId { get; set; }

        Guid IAggregateCommand.AggregateId
        {
            get { return AssetFolderId; }
        }
    }
}
