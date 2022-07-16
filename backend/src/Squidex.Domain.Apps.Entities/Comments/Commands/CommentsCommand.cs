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
    public abstract class CommentsCommand : SquidexCommand, IAppCommand, IAggregateCommand
    {
        public static readonly NamedId<DomainId> NoApp = NamedId.Of(DomainId.NewGuid(), "none");

        public NamedId<DomainId> AppId { get; set; }

        public DomainId CommentsId { get; set; }

        public DomainId CommentId { get; set; }

        DomainId IAggregateCommand.AggregateId
        {
            get => AppId.Id != default ? DomainId.Combine(AppId.Id, CommentsId) : CommentsId;
        }
    }
}
