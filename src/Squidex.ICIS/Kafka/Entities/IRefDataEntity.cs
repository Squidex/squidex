using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.ICIS.Kafka.Entities
{
    public interface IRefDataEntity
    {
        string Id { get; }

        string IdField { get; }

        string Schema { get; }

        NamedContentData ToData();
    }
}
