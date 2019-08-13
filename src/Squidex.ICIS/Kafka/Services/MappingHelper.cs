// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using System;

namespace Squidex.ICIS.Kafka.Services
{
    public static class MappingHelper
    {
        public static string GetInvariantString(this NamedContentData data, string field)
        {
            if (!data.TryGetValue(field, out var fieldValue))
            {
                throw new ArgumentException($"Cannot find field '{field}' in data.", nameof(data));
            }

            if(!fieldValue.TryGetValue("iv", out var value))
            {
                throw new ArgumentException($"Cannot find invariant value in field '{field}'.", nameof(data));
            }

            return value.ToString();
        }
    }
}
