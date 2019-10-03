// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json;
using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.ICIS.Kafka.Entities
{
    [TopicName("{environment}_iddn_location_external_1")]
    public sealed class Location : IRefDataEntity
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public LocalizedValue Name { get; set; }

        public string IdField => "id";

        public string Schema => "region";

        public NamedContentData ToData()
        {
            return new NamedContentData()
                .AddField("id",
                    new ContentFieldData()
                        .AddValue(Id))
                .AddField("name", Name.ToFieldData());
        }
    }
}
