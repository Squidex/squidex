// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Comments.Commands
{
    public abstract class CommentsCommand : SquidexCommand, IAppCommand
    {
        public string CommentsId { get; set; }

        public DomainId CommentId { get; set; }

        public NamedId<DomainId> AppId { get; set; }
    }
}
