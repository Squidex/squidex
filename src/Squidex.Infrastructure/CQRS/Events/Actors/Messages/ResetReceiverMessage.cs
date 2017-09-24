using Squidex.Infrastructure.Actors;

namespace Squidex.Infrastructure.CQRS.Events.Actors.Messages
{
    [TypeName(nameof(ResetReceiverMessage))]
    public sealed class ResetReceiverMessage : IMessage
    {
    }
}
