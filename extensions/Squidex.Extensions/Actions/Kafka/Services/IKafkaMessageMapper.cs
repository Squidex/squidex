// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Avro.Specific;
using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Extensions.Actions.Kafka
{
    public interface IKafkaMessageMapper
    {
        ISpecificRecord ToAvro(NamedContentData namedContentData);
    }
}
