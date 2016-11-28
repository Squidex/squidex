// ==========================================================================
//  EnrichWithAppIdProcessor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Events;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Write
{
    public sealed class EnrichWithAppIdProcessor : IEventProcessor
    {
        public Task ProcessEventAsync(Envelope<IEvent> @event, IAggregate aggregate, ICommand command)
        {
            var appCommand = command as IAppCommand;

            if (appCommand != null)
            {
                @event.SetAppId(appCommand.AppId);
            }

            return TaskHelper.Done;
        }
    }
}
