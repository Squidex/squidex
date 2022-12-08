// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.EventConsumers.Models;

public sealed class EventConsumerDto : Resource
{
    /// <summary>
    /// Indicates if the event consumer has been started.
    /// </summary>
    public bool IsStopped { get; set; }

    /// <summary>
    /// Indicates if the event consumer is resetting at the moment.
    /// </summary>
    public bool IsResetting { get; set; }

    /// <summary>
    /// The number of handled events.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// The name of the event consumer.
    /// </summary>
    [LocalizedRequired]
    public string Name { get; set; }

    /// <summary>
    /// The error details if the event consumer has been stopped after a failure.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// The position within the vent stream.
    /// </summary>
    public string? Position { get; set; }

    public static EventConsumerDto FromDomain(EventConsumerInfo eventConsumerInfo, Resources resources)
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
                AddPutLink("reset",
                    resources.Url<EventConsumersController>(x => nameof(x.ResetEventConsumer), values));
            }

            if (IsStopped)
            {
                AddPutLink("start",
                    resources.Url<EventConsumersController>(x => nameof(x.StartEventConsumer), values));
            }
            else
            {
                AddPutLink("stop",
                    resources.Url<EventConsumersController>(x => nameof(x.StopEventConsumer), values));
            }
        }

        return this;
    }
}
