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

        public static EventConsumersDto FromResults(IEnumerable<EventConsumerInfo> items, Resources resources)
        {
            var result = new EventConsumersDto
            {
                Items = items.Select(x => EventConsumerDto.FromEventConsumerInfo(x, resources)).ToArray()
            };

            return result.CreateLinks(resources);
        }

        private EventConsumersDto CreateLinks(Resources resources)
        {
            AddSelfLink(resources.Url<EventConsumersController>(c => nameof(c.GetEventConsumers)));

            return this;
        }
    }
}
