// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Comments.Commands
{
    public abstract class CommentsCommand : SquidexCommand, IAggregateCommand, IAppCommand
    {
        public Guid CommentsId { get; set; }

        public NamedId<Guid> AppId { get; set; }

        Guid IAggregateCommand.AggregateId
        {
            get { return CommentsId; }
        }
    }
}
