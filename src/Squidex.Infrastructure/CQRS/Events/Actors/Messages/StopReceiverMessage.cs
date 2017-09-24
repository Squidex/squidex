using System;
using Squidex.Infrastructure.Actors;

namespace Squidex.Infrastructure.CQRS.Events.Actors.Messages
{
    [TypeName(nameof(StopReceiverMessage))]
    public sealed class StopReceiverMessage : IMessage
    {
        public Exception Exception { get; set; }
    }
}
