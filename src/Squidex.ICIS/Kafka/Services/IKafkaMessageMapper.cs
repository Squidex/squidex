// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Avro.Specific;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.HandleRules.EnrichedEvents;

namespace Squidex.ICIS.Actions.Kafka
{
    public interface IKafkaMessageMapper
    {
        ISpecificRecord ToAvro(EnrichedContentEvent namedContentData);
    }
}
