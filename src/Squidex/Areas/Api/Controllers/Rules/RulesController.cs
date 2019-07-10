// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using NodaTime;
using Squidex.Areas.Api.Controllers.Rules.Models;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Rules
{
    /// <summary>
    /// Manages and retrieves information about schemas.
    /// </summary>
    [ApiExplorerSettings(GroupName = nameof(Rules))]
    public sealed class RulesController : ApiController
    {
        private readonly IAppProvider appProvider;
        private readonly IRuleEventRepository ruleEventsRepository;
        private readonly RuleRegistry ruleRegistry;

        public RulesController(ICommandBus commandBus, IAppProvider appProvider,
            IRuleEventRepository ruleEventsRepository, RuleRegistry ruleRegistry)
            : base(commandBus)
        {
            this.appProvider = appProvider;

            this.ruleEventsRepository = ruleEventsRepository;
            this.ruleRegistry = ruleRegistry;
        }

        /// <summary>
        /// Get the supported rule actions.
        /// </summary>
        /// <returns>
        /// 200 => Rule actions returned.
        /// </returns>
        [HttpGet]
        [Route("rules/actions/")]
        [ProducesResponseType(typeof(Dictionary<string, RuleElementDto>), 200)]
        [ApiPermission]
        [ApiCosts(0)]
        public IActionResult GetActions()
        {
            var etag = string.Concat(ruleRegistry.Actions.Select(x => x.Key)).Sha256Base64();

            var response = Deferred.Response(() =>
            {
                return ruleRegistry.Actions.ToDictionary(x => x.Key, x => RuleElementDto.FromDefinition(x.Value));
            });

            Response.Headers[HeaderNames.ETag] = etag;

            return Ok(response);
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
        [ProducesResponseType(typeof(RulesDto), 200)]
        [ApiPermission(Permissions.AppRulesRead)]
        [ApiCosts(1)]
        public async Task<IActionResult> GetRules(string app)
        {
            var rules = await appProvider.GetRulesAsync(AppId);

            var response = Deferred.Response(() =>
            {
                return RulesDto.FromRules(rules, this, app);
            });

            Response.Headers[HeaderNames.ETag] = rules.ToEtag();

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
        [ProducesResponseType(typeof(RuleDto), 201)]
        [ApiPermission(Permissions.AppRulesCreate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostRule(string app, [FromBody] CreateRuleDto request)
        {
            var command = request.ToCommand();

            var response = await InvokeCommandAsync(app, command);

            return CreatedAtAction(nameof(GetRules), new { app }, response);
        }

        /// <summary>
        /// Update a rule.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the rule to update.</param>
        /// <param name="request">The rule object that needs to be added to the app.</param>
        /// <returns>
        /// 200 => Rule updated.
        /// 400 => Rule is not valid.
        /// 404 => Rule or app not found.
        /// </returns>
        /// <remarks>
        /// All events for the specified schemas will be sent to the url. The timeout is 2 seconds.
        /// </remarks>
        [HttpPut]
        [Route("apps/{app}/rules/{id}/")]
        [ProducesResponseType(typeof(RuleDto), 400)]
        [ApiPermission(Permissions.AppRulesUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutRule(string app, Guid id, [FromBody] UpdateRuleDto request)
        {
            var command = request.ToCommand(id);

            var response = await InvokeCommandAsync(app, command);

            return Ok(response);
        }

        /// <summary>
        /// Enable a rule.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the rule to enable.</param>
        /// <returns>
        /// 200 => Rule enabled.
        /// 400 => Rule already enabled.
        /// 404 => Rule or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/rules/{id}/enable/")]
        [ProducesResponseType(typeof(RuleDto), 200)]
        [ApiPermission(Permissions.AppRulesDisable)]
        [ApiCosts(1)]
        public async Task<IActionResult> EnableRule(string app, Guid id)
        {
            var command = new EnableRule { RuleId = id };

            var response = await InvokeCommandAsync(app, command);

            return Ok(response);
        }

        /// <summary>
        /// Disable a rule.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the rule to disable.</param>
        /// <returns>
        /// 200 => Rule disabled.
        /// 400 => Rule already disabled.
        /// 404 => Rule or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/rules/{id}/disable/")]
        [ProducesResponseType(typeof(RuleDto), 200)]
        [ApiPermission(Permissions.AppRulesDisable)]
        [ApiCosts(1)]
        public async Task<IActionResult> DisableRule(string app, Guid id)
        {
            var command = new DisableRule { RuleId = id };

            var response = await InvokeCommandAsync(app, command);

            return Ok(response);
        }

        /// <summary>
        /// Delete a rule.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the rule to delete.</param>
        /// <returns>
        /// 204 => Rule deleted.
        /// 404 => Rule or app not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/rules/{id}/")]
        [ApiPermission(Permissions.AppRulesDelete)]
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
        [ApiPermission(Permissions.AppRulesRead)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetEvents(string app, [FromQuery] int skip = 0, [FromQuery] int take = 20)
        {
            var taskForItems = ruleEventsRepository.QueryByAppAsync(AppId, skip, take);
            var taskForCount = ruleEventsRepository.CountByAppAsync(AppId);

            await Task.WhenAll(taskForItems, taskForCount);

            var response = RuleEventsDto.FromRuleEvents(taskForItems.Result, taskForCount.Result, this, app);

            return Ok(response);
        }

        /// <summary>
        /// Retry the event immediately.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The event to enqueue.</param>
        /// <returns>
        /// 204 => Rule enqueued.
        /// 404 => App or rule event not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/rules/events/{id}/")]
        [ApiPermission(Permissions.AppRulesEvents)]
        [ApiCosts(0)]
        public async Task<IActionResult> PutEvent(string app, Guid id)
        {
            var ruleEvent = await ruleEventsRepository.FindAsync(id);

            if (ruleEvent == null)
            {
                return NotFound();
            }

            await ruleEventsRepository.EnqueueAsync(id, SystemClock.Instance.GetCurrentInstant());

            return NoContent();
        }

        /// <summary>
        /// Cancels the event and retries.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The event to enqueue.</param>
        /// <returns>
        /// 204 => Rule deqeued.
        /// 404 => App or rule event not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/rules/events/{id}/")]
        [ApiPermission(Permissions.AppRulesEvents)]
        [ApiCosts(0)]
        public async Task<IActionResult> DeleteEvent(string app, Guid id)
        {
            var ruleEvent = await ruleEventsRepository.FindAsync(id);

            if (ruleEvent == null)
            {
                return NotFound();
            }

            await ruleEventsRepository.CancelAsync(id);

            return NoContent();
        }

        private async Task<RuleDto> InvokeCommandAsync(string app, ICommand command)
        {
            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<IRuleEntity>();
            var response = RuleDto.FromRule(result, this, app);

            return response;
        }
    }
}