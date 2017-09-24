using Squidex.Infrastructure.Actors;

namespace Squidex.Infrastructure.CQRS.Events.Actors.Messages
{
    [TypeName(nameof(StartReceiverMessage))]
    public sealed class StartReceiverMessage : IMessage
    {
    }
}
