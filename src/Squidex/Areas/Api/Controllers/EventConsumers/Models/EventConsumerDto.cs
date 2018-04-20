// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.EventConsumers.Models
{
    public sealed class EventConsumerDto
    {
        public bool IsStopped { get; set; }

        public bool IsResetting { get; set; }

        public string Name { get; set; }

        public string Error { get; set; }

        public string Position { get; set; }

        public static EventConsumerDto FromEventConsumerInfo(EventConsumerInfo eventConsumerInfo)
        {
            return SimpleMapper.Map(eventConsumerInfo, new EventConsumerDto());
        }
    }
}
