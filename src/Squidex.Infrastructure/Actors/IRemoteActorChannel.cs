using System;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Actors
{
    public interface IRemoteActorChannel
    {
        Task SendAsync(string recipient, IMessage message);

        void Subscribe(string recipient, Action<IMessage> handler);
    }
}