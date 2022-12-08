// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Mvc;
using Squidex.Areas.Api.Controllers.EventConsumers.Models;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.EventSourcing.Consume;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.EventConsumers;

/// <summary>
/// Update and query event consumers.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(EventConsumers))]
public sealed class EventConsumersController : ApiController
{
    private readonly IEventConsumerManager eventConsumerManager;

    public EventConsumersController(ICommandBus commandBus, IEventConsumerManager eventConsumerManager)
        : base(commandBus)
    {
        this.eventConsumerManager = eventConsumerManager;
    }

    /// <summary>
    /// Get event consumers.
    /// </summary>
    /// <response code="200">Event consumers returned.</response>.
    [HttpGet]
    [Route("event-consumers/")]
    [ProducesResponseType(typeof(EventConsumersDto), StatusCodes.Status200OK)]
    [ApiPermission(PermissionIds.AdminEventsRead)]
    public async Task<IActionResult> GetEventConsumers()
    {
        var eventConsumers = await eventConsumerManager.GetConsumersAsync(HttpContext.RequestAborted);

        var response = EventConsumersDto.FromDomain(eventConsumers, Resources);

        return Ok(response);
    }

    /// <summary>
    /// Start an event consumer.
    /// </summary>
    /// <param name="consumerName">The name of the event consumer.</param>
    /// <response code="200">Event consumer started asynchronously.</response>.
    /// <response code="404">Event consumer not found.</response>.
    [HttpPut]
    [Route("event-consumers/{consumerName}/start/")]
    [ProducesResponseType(typeof(EventConsumerDto), StatusCodes.Status200OK)]
    [ApiPermission(PermissionIds.AdminEventsManage)]
    public async Task<IActionResult> StartEventConsumer(string consumerName)
    {
        var eventConsumer = await eventConsumerManager.StartAsync(consumerName, HttpContext.RequestAborted);

        var response = EventConsumerDto.FromDomain(eventConsumer, Resources);

        return Ok(response);
    }

    /// <summary>
    /// Stop an event consumer.
    /// </summary>
    /// <param name="consumerName">The name of the event consumer.</param>
    /// <response code="200">Event consumer stopped asynchronously.</response>.
    /// <response code="404">Event consumer not found.</response>.
    [HttpPut]
    [Route("event-consumers/{consumerName}/stop/")]
    [ProducesResponseType(typeof(EventConsumerDto), StatusCodes.Status200OK)]
    [ApiPermission(PermissionIds.AdminEventsManage)]
    public async Task<IActionResult> StopEventConsumer(string consumerName)
    {
        var eventConsumer = await eventConsumerManager.StopAsync(consumerName, HttpContext.RequestAborted);

        var response = EventConsumerDto.FromDomain(eventConsumer, Resources);

        return Ok(response);
    }

    /// <summary>
    /// Reset an event consumer.
    /// </summary>
    /// <param name="consumerName">The name of the event consumer.</param>
    /// <response code="200">Event consumer resetted asynchronously.</response>.
    /// <response code="404">Event consumer not found.</response>.
    [HttpPut]
    [Route("event-consumers/{consumerName}/reset/")]
    [ProducesResponseType(typeof(EventConsumerDto), StatusCodes.Status200OK)]
    [ApiPermission(PermissionIds.AdminEventsManage)]
    public async Task<IActionResult> ResetEventConsumer(string consumerName)
    {
        var eventConsumer = await eventConsumerManager.ResetAsync(consumerName, HttpContext.RequestAborted);

        var response = EventConsumerDto.FromDomain(eventConsumer, Resources);

        return Ok(response);
    }
}
