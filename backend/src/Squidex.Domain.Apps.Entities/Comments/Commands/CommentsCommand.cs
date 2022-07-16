// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Comments.Commands
{
    public abstract class CommentsCommand : SquidexCommand, IAggregateCommand
    {
        public DomainId CommentsId { get; set; }

        public DomainId CommentId { get; set; }

        DomainId IAggregateCommand.AggregateId => CommentsId;
    }
}
