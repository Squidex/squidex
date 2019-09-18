using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.Contents;
using System.Collections.Generic;

namespace Squidex.ICIS.Kafka.Entities
{
    public sealed class LocalizedValue
    {
        [JsonProperty("l10n")]
        public List<LocalizedValueText> Texts { get; set; }

        public ContentFieldData ToFieldData()
        {
            var result = new ContentFieldData();

            foreach (var item in Texts)
            {
                result.AddValue(item.Language, item.Text);
            }

            return result;
        }
    }
}
