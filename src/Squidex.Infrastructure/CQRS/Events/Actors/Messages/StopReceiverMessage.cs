using System;
using Squidex.Infrastructure.Actors;

namespace Squidex.Infrastructure.CQRS.Events.Actors.Messages
{
    public sealed class StopReceiverMessage : IMessage
    {
        public Exception Exception { get; set; }
    }
}
