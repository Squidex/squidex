namespace Squidex.ICIS.Kafka.Consumer
{
    public class ConsumerOptions
    {
        public string GroupId { get; set; }

        public string AppName { get; set; }

        public string ClientName { get; set; }

        public string Environment { get; set; } = "integ";
    }
}
