// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.EventConsumers.Models
{
    public sealed class EventConsumerDto : Resource
    {
        public bool IsStopped { get; set; }

        public bool IsResetting { get; set; }

        public string Name { get; set; }

        public string Error { get; set; }

        public string Position { get; set; }

        public static EventConsumerDto FromEventConsumerInfo(EventConsumerInfo eventConsumerInfo, ApiController controller)
        {
            var result = SimpleMapper.Map(eventConsumerInfo, new EventConsumerDto());

            return result.CreateLinks(controller);
        }

        private EventConsumerDto CreateLinks(ApiController controller)
        {
            if (controller.HasPermission(Permissions.AdminEventsManage))
            {
                var values = new { name = Name };

                if (!IsResetting)
                {
                    AddPutLink("reset", controller.Url<EventConsumersController>(x => nameof(x.ResetEventConsumer), values));
                }

                if (IsStopped)
                {
                    AddPutLink("start", controller.Url<EventConsumersController>(x => nameof(x.StartEventConsumer), values));
                }
                else
                {
                    AddPutLink("stop", controller.Url<EventConsumersController>(x => nameof(x.StopEventConsumer), values));
                }
            }

            return this;
        }
    }
}
