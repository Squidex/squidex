namespace Squidex.ICIS.Kafka.Consumer
{
    public sealed class CommodityConsumerOptions
    {
        public string AppName { get; set; }

        public string SchemaName { get; set; }

        public string ClientName { get; set; }
    }
}
