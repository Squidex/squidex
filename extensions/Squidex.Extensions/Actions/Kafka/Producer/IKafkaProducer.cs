// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Avro.Specific;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using Newtonsoft.Json;

namespace Squidex.Extensions.Actions.Kafka
{
    public interface IKafkaProducer<T> : IDisposable where T : ISpecificRecord
    {
        Task<DeliveryResult<string, T>> Send(string key, T val);
    }
}