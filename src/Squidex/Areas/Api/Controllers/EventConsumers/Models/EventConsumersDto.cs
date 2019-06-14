// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.EventConsumers.Models
{
    public sealed class EventConsumersDto : Resource
    {
        /// <summary>
        /// The event consumers.
        /// </summary>
        public EventConsumerDto[] Items { get; set; }

        public static EventConsumersDto FromResults(IEnumerable<EventConsumerInfo> items, ApiController controller)
        {
            var result = new EventConsumersDto
            {
                Items = items.Select(x => EventConsumerDto.FromEventConsumerInfo(x, controller)).ToArray()
            };

            return result.CreateLinks(controller);
        }

        private EventConsumersDto CreateLinks(ApiController controller)
        {
            AddSelfLink(controller.Url<EventConsumersController>(c => nameof(c.GetEventConsumers)));

            return this;
        }
    }
}
