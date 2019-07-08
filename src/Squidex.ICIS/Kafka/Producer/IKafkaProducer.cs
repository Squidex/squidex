﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.ICIS.Actions.Kafka
{
    using System;
    using System.Threading.Tasks;
    using Avro.Specific;
    using Confluent.Kafka;

    public interface IKafkaProducer<T> : IDisposable where T : ISpecificRecord
    {
        Task<DeliveryResult<string, T>> Send(string topicName, string key, T val);
    }
}