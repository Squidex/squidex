using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure;
using System.Threading.Tasks;

namespace Squidex.ICIS.Kafka.Consumer
{
    public interface IKafkaHandler<T>
    {
        Task HandleAsync(RefToken actor, Context context, string key, T consumed);
    }
}
