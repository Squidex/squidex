// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Comments.Commands
{
    public abstract class CommentsCommand : SquidexCommand, IAppCommand
    {
        public NamedId<DomainId> AppId { get; set; }

        public DomainId CommentsId { get; set; }

        public DomainId CommentId { get; set; }
    }
}
