// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.EventConsumers.Models
{
    public sealed class EventConsumerDto : Resource
    {
        public bool IsStopped { get; set; }

        public bool IsResetting { get; set; }

        public int Count { get; set; }

        public string Name { get; set; }

        public string? Error { get; set; }

        public string? Position { get; set; }

        public static EventConsumerDto FromEventConsumerInfo(EventConsumerInfo eventConsumerInfo, Resources resources)
        {
            var result = SimpleMapper.Map(eventConsumerInfo, new EventConsumerDto());

            return result.CreateLinks(resources);
        }

        private EventConsumerDto CreateLinks(Resources resources)
        {
            if (resources.CanManageEvents)
            {
                var values = new { consumerName = Name };

                if (!IsResetting)
                {
                    AddPutLink("reset", resources.Url<EventConsumersController>(x => nameof(x.ResetEventConsumer), values));
                }

                if (IsStopped)
                {
                    AddPutLink("start", resources.Url<EventConsumersController>(x => nameof(x.StartEventConsumer), values));
                }
                else
                {
                    AddPutLink("stop", resources.Url<EventConsumersController>(x => nameof(x.StopEventConsumer), values));
                }
            }

            return this;
        }
    }
}
