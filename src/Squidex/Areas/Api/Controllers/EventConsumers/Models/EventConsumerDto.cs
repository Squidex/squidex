// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Security;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.EventConsumers.Models
{
    public sealed class EventConsumerDto : Resource
    {
        private static readonly Permission EventsManagePermission = new Permission(Permissions.AdminEventsManage);

        public bool IsStopped { get; set; }

        public bool IsResetting { get; set; }

        public string Name { get; set; }

        public string Error { get; set; }

        public string Position { get; set; }

        public static EventConsumerDto FromEventConsumerInfo(EventConsumerInfo eventConsumerInfo, ApiController controller)
        {
            var result = SimpleMapper.Map(eventConsumerInfo, new EventConsumerDto());

            return CreateLinks(result, controller);
        }

        private static EventConsumerDto CreateLinks(EventConsumerDto result, ApiController controller)
        {
            if (controller.HasPermission(EventsManagePermission))
            {
                var values = new { name = result.Name };

                if (!result.IsResetting)
                {
                    result.AddPutLink("reset", controller.Url<EventConsumersController>(x => nameof(x.ResetEventConsumer), values));
                }

                if (result.IsStopped)
                {
                    result.AddPutLink("start", controller.Url<EventConsumersController>(x => nameof(x.StartEventConsumer), values));
                }
                else
                {
                    result.AddPutLink("stop", controller.Url<EventConsumersController>(x => nameof(x.StopEventConsumer), values));
                }
            }

            return result;
        }
    }
}
