using Squidex.Infrastructure.Actors;

namespace Squidex.Infrastructure.CQRS.Events.Actors.Messages
{
    public sealed class SubscribeMessage : IMessage
    {
        public IActor Parent { get; set; }
    }
}
