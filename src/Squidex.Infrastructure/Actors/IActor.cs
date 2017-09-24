using System;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Actors
{
    public interface IActor
    {
        Task SendAsync(IMessage message);

        Task SendAsync(Exception exception);

        Task StopAsync();
    }
}