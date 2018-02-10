// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Contents.Commands
{
    public abstract class ContentCommand : SquidexCommand, IAggregateCommand
    {
        public Guid ContentId { get; set; }

        Guid IAggregateCommand.AggregateId
        {
            get { return ContentId; }
        }
    }
}
