// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Infrastructure.Commands
{
    public interface IDomainState<T>
    {
        long Version { get; set; }

        T Apply(Envelope<IEvent> @event);
    }
}
