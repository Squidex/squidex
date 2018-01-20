// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using NSwag.Annotations;
using Squidex.Areas.Api.Controllers.Rules.Models;
using Squidex.Areas.Api.Controllers.Rules.Models.Converters;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;

namespace Squidex.Areas.Api.Controllers.Rules
{
    /// <summary>
    /// Manages and retrieves information about schemas.
    /// </summary>
    [ApiAuthorize]
    [ApiExceptionFilter]
    [AppApi]
    [SwaggerTag(nameof(Rules))]
    [MustBeAppDeveloper]
    public sealed class RulesController : ApiController
    {
        private readonly IAppProvider appProvider;
        private readonly IRuleEventRepository ruleEventsRepository;

        public RulesController(ICommandBus commandBus, IAppProvider appProvider,
            IRuleEventRepository ruleEventsRepository)
            : base(commandBus)
        {
            this.appProvider = appProvider;

            this.ruleEventsRepository = ruleEventsRepository;
        }

        /// <summary>
        /// Get rules.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 200 => Rules returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/rules/")]
        [ProducesResponseType(typeof(RuleDto[]), 200)]
        [ApiCosts(1)]
        public async Task<IActionResult> GetRules(string app)
        {
            var rules = await appProvider.GetRulesAsync(AppId);

            var response = rules.Select(r => r.ToModel());

            return Ok(response);
        }

        /// <summary>
        /// Create a new rule.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="request">The rule object that needs to be added to the app.</param>
        /// <returns>
        /// 201 => Rule created.
        /// 400 => Rule is not valid.
        /// 404 => App not found.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/rules/")]
        [ProducesResponseType(typeof(EntityCreatedDto), 201)]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostRule(string app, [FromBody] CreateRuleDto request)
        {
            var command = request.ToCommand();

            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<EntityCreatedResult<Guid>>();
            var response = new EntityCreatedDto { Id = result.IdOrValue.ToString(), Version = result.Version };

            return CreatedAtAction(nameof(GetRules), new { app }, response);
        }

        /// <summary>
        /// Update a rule.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the rule to update.</param>
        /// <param name="request">The rule object that needs to be added to the app.</param>
        /// <returns>
        /// 204 => Rule updated.
        /// 400 => Rule is not valid.
        /// 404 => Rule or app not found.
        /// </returns>
        /// <remarks>
        /// All events for the specified schemas will be sent to the url. The timeout is 2 seconds.
        /// </remarks>
        [HttpPut]
        [Route("apps/{app}/rules/{id}/")]
        [ProducesResponseType(typeof(ErrorDto), 400)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutRule(string app, Guid id, [FromBody] UpdateRuleDto request)
        {
            var command = request.ToCommand(id);

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        /// <summary>
        /// Enable a rule.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the rule to enable.</param>
        /// <returns>
        /// 204 => Rule enabled.
        /// 400 => Rule already enabled.
        /// 404 => Rule or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/rules/{id}/enable/")]
        [ApiCosts(1)]
        public async Task<IActionResult> EnableRule(string app, Guid id)
        {
            await CommandBus.PublishAsync(new EnableRule { RuleId = id });

            return NoContent();
        }

        /// <summary>
        /// Disable a rule.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the rule to disable.</param>
        /// <returns>
        /// 204 => Rule disabled.
        /// 400 => Rule already disabled.
        /// 404 => Rule or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/rules/{id}/disable/")]
        [ApiCosts(1)]
        public async Task<IActionResult> DisableRule(string app, Guid id)
        {
            await CommandBus.PublishAsync(new DisableRule { RuleId = id });

            return NoContent();
        }

        /// <summary>
        /// Delete a rule.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the rule to delete.</param>
        /// <returns>
        /// 204 => Rule has been deleted.
        /// 404 => Rule or app not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/rules/{id}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteRule(string app, Guid id)
        {
            await CommandBus.PublishAsync(new DeleteRule { RuleId = id });

            return NoContent();
        }

        /// <summary>
        /// Get rule events.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="skip">The number of events to skip.</param>
        /// <param name="take">The number of events to take.</param>
        /// <returns>
        /// 200 => Rule events returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/rules/events/")]
        [ProducesResponseType(typeof(RuleEventsDto), 200)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetEvents(string app, [FromQuery] int skip = 0, [FromQuery] int take = 20)
        {
            var taskForItems = ruleEventsRepository.QueryByAppAsync(App.Id, skip, take);
            var taskForCount = ruleEventsRepository.CountByAppAsync(App.Id);

            await Task.WhenAll(taskForItems, taskForCount);

            var response = new RuleEventsDto
            {
                Total = taskForCount.Result,
                Items = taskForItems.Result.Select(x =>
                {
                    var itemModel = new RuleEventDto();

                    SimpleMapper.Map(x, itemModel);
                    SimpleMapper.Map(x.Job, itemModel);

                    return itemModel;
                }).ToArray()
            };

            return Ok(response);
        }

        /// <summary>
        /// Enqueue the event to be send.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The event to enqueue.</param>
        /// <returns>
        /// 200 => Rule enqueued.
        /// 404 => App or rule event not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/rules/events/{id}/")]
        [ApiCosts(0)]
        public async Task<IActionResult> PutEvent(string app, Guid id)
        {
            var entity = await ruleEventsRepository.FindAsync(id);

            if (entity == null)
            {
                return NotFound();
            }

            await ruleEventsRepository.EnqueueAsync(id, SystemClock.Instance.GetCurrentInstant());

            return NoContent();
        }
    }
}
