// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Comments.State
{
    public sealed class CommentsState : DomainObjectState<CommentsState>
    {
        public override CommentsState Apply(Envelope<IEvent> @event)
        {
            return this;
        }
    }
}
