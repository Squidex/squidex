// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Avro.Specific;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;

namespace Squidex.ICIS.Kafka.Services
{
    public interface IKafkaMessageMapper
    {
        ISpecificRecord ToAvro(EnrichedContentEvent namedContentData);
    }
}
