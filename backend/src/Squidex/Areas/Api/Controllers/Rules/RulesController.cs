// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using NodaTime;
using Squidex.Areas.Api.Controllers.Rules.Models;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Domain.Apps.Entities.Rules.Runner;
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
        private readonly EventJsonSchemaGenerator eventJsonSchemaGenerator;
        private readonly IAppProvider appProvider;
        private readonly IRuleEventRepository ruleEventsRepository;
        private readonly IRuleQueryService ruleQuery;
        private readonly IRuleRunnerService ruleRunnerService;
        private readonly RuleRegistry ruleRegistry;

        public RulesController(ICommandBus commandBus,
            IAppProvider appProvider,
            IRuleEventRepository ruleEventsRepository,
            IRuleQueryService ruleQuery,
            IRuleRunnerService ruleRunnerService,
            RuleRegistry ruleRegistry,
            EventJsonSchemaGenerator eventJsonSchemaGenerator)
            : base(commandBus)
        {
            this.appProvider = appProvider;
            this.ruleEventsRepository = ruleEventsRepository;
            this.ruleQuery = ruleQuery;
            this.ruleRunnerService = ruleRunnerService;
            this.ruleRegistry = ruleRegistry;
            this.eventJsonSchemaGenerator = eventJsonSchemaGenerator;
        }

        /// <summary>
        /// Get supported rule actions.
        /// </summary>
        /// <returns>
        /// 200 => Rule actions returned.
        /// </returns>
        [HttpGet]
        [Route("rules/actions/")]
        [ProducesResponseType(typeof(Dictionary<string, RuleElementDto>), StatusCodes.Status200OK)]
        [ApiPermission]
        [ApiCosts(0)]
        public IActionResult GetActions()
        {
            var etag = string.Concat(ruleRegistry.Actions.Select(x => x.Key)).ToSha256Base64();

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
        [ProducesResponseType(typeof(RulesDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppRulesRead)]
        [ApiCosts(1)]
        public async Task<IActionResult> GetRules(string app)
        {
            var rules = await ruleQuery.QueryAsync(Context, HttpContext.RequestAborted);

            var response = Deferred.AsyncResponse(() =>
            {
                return RulesDto.FromRulesAsync(rules, ruleRunnerService, Resources);
            });

            return Ok(response);
        }

        /// <summary>
        /// Create a new rule.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="request">The rule object that needs to be added to the app.</param>
        /// <returns>
        /// 201 => Rule created.
        /// 400 => Rule request not valid.
        /// 404 => App not found.
        /// </returns>
        [HttpPost]
        [Route("apps/{app}/rules/")]
        [ProducesResponseType(typeof(RuleDto), 201)]
        [ApiPermissionOrAnonymous(Permissions.AppRulesCreate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PostRule(string app, [FromBody] CreateRuleDto request)
        {
            var command = request.ToCommand();

            var response = await InvokeCommandAsync(command);

            return CreatedAtAction(nameof(GetRules), new { app }, response);
        }

        /// <summary>
        /// Cancel the current run.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 204 => Rule run cancelled.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/rules/run")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ApiPermissionOrAnonymous(Permissions.AppRulesEvents)]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteRuleRun(string app)
        {
            await ruleRunnerService.CancelAsync(App.Id, HttpContext.RequestAborted);

            return NoContent();
        }

        /// <summary>
        /// Update a rule.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the rule to update.</param>
        /// <param name="request">The rule object that needs to be added to the app.</param>
        /// <returns>
        /// 200 => Rule updated.
        /// 400 => Rule request not valid.
        /// 404 => Rule or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/rules/{id}/")]
        [ProducesResponseType(typeof(RuleDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppRulesUpdate)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutRule(string app, DomainId id, [FromBody] UpdateRuleDto request)
        {
            var command = request.ToCommand(id);

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Enable a rule.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the rule to enable.</param>
        /// <returns>
        /// 200 => Rule enabled.
        /// 404 => Rule or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/rules/{id}/enable/")]
        [ProducesResponseType(typeof(RuleDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppRulesDisable)]
        [ApiCosts(1)]
        public async Task<IActionResult> EnableRule(string app, DomainId id)
        {
            var command = new EnableRule { RuleId = id };

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Disable a rule.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the rule to disable.</param>
        /// <returns>
        /// 200 => Rule disabled.
        /// 404 => Rule or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/rules/{id}/disable/")]
        [ProducesResponseType(typeof(RuleDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppRulesDisable)]
        [ApiCosts(1)]
        public async Task<IActionResult> DisableRule(string app, DomainId id)
        {
            var command = new DisableRule { RuleId = id };

            var response = await InvokeCommandAsync(command);

            return Ok(response);
        }

        /// <summary>
        /// Trigger a rule.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the rule to disable.</param>
        /// <returns>
        /// 204 => Rule triggered.
        /// 404 => Rule or app not found.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/rules/{id}/trigger/")]
        [ApiPermissionOrAnonymous(Permissions.AppRulesEvents)]
        [ApiCosts(1)]
        public async Task<IActionResult> TriggerRule(string app, DomainId id)
        {
            var command = new TriggerRule { RuleId = id };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        /// <summary>
        /// Run a rule.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the rule to run.</param>
        /// <param name="fromSnapshots">Runs the rule from snapeshots if possible.</param>
        /// <returns>
        /// 204 => Rule started.
        /// </returns>
        [HttpPut]
        [Route("apps/{app}/rules/{id}/run")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ApiPermissionOrAnonymous(Permissions.AppRulesEvents)]
        [ApiCosts(1)]
        public async Task<IActionResult> PutRuleRun(string app, DomainId id, [FromQuery] bool fromSnapshots = false)
        {
            await ruleRunnerService.RunAsync(App.Id, id, fromSnapshots, HttpContext.RequestAborted);

            return NoContent();
        }

        /// <summary>
        /// Cancels all rule events.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the rule to cancel.</param>
        /// <returns>
        /// 204 => Rule events cancelled.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/rules/{id}/events/")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ApiPermissionOrAnonymous(Permissions.AppRulesEvents)]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteRuleEvents(string app, DomainId id)
        {
            await ruleEventsRepository.CancelByRuleAsync(id, HttpContext.RequestAborted);

            return NoContent();
        }

        /// <summary>
        /// Simulate a rule.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the rule to simulate.</param>
        /// <returns>
        /// 200 => Rule simulated.
        /// 404 => Rule or app not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/rules/{id}/simulate/")]
        [ProducesResponseType(typeof(SimulatedRuleEventsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppRulesEvents)]
        [ApiCosts(5)]
        public async Task<IActionResult> Simulate(string app, DomainId id)
        {
            var rule = await appProvider.GetRuleAsync(AppId, id, HttpContext.RequestAborted);

            if (rule == null)
            {
                return NotFound();
            }

            var simulation = await ruleRunnerService.SimulateAsync(rule, HttpContext.RequestAborted);

            var response = SimulatedRuleEventsDto.FromSimulatedRuleEvents(simulation);

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
        [ApiPermissionOrAnonymous(Permissions.AppRulesDelete)]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteRule(string app, DomainId id)
        {
            await CommandBus.PublishAsync(new DeleteRule { RuleId = id });

            return NoContent();
        }

        /// <summary>
        /// Get rule events.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="ruleId">The optional rule id to filter to events.</param>
        /// <param name="skip">The number of events to skip.</param>
        /// <param name="take">The number of events to take.</param>
        /// <returns>
        /// 200 => Rule events returned.
        /// 404 => App not found.
        /// </returns>
        [HttpGet]
        [Route("apps/{app}/rules/events/")]
        [ProducesResponseType(typeof(RuleEventsDto), StatusCodes.Status200OK)]
        [ApiPermissionOrAnonymous(Permissions.AppRulesRead)]
        [ApiCosts(0)]
        public async Task<IActionResult> GetEvents(string app, [FromQuery] DomainId? ruleId = null, [FromQuery] int skip = 0, [FromQuery] int take = 20)
        {
            var ruleEvents = await ruleEventsRepository.QueryByAppAsync(AppId, ruleId, skip, take, HttpContext.RequestAborted);

            var response = RuleEventsDto.FromRuleEvents(ruleEvents, Resources, ruleId);

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
        [ApiPermissionOrAnonymous(Permissions.AppRulesEvents)]
        [ApiCosts(0)]
        public async Task<IActionResult> PutEvent(string app, DomainId id)
        {
            var ruleEvent = await ruleEventsRepository.FindAsync(id, HttpContext.RequestAborted);

            if (ruleEvent == null)
            {
                return NotFound();
            }

            await ruleEventsRepository.EnqueueAsync(id, SystemClock.Instance.GetCurrentInstant(), HttpContext.RequestAborted);

            return NoContent();
        }

        /// <summary>
        /// Cancels all events.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <returns>
        /// 204 => Events cancelled.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/rules/events/")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ApiPermissionOrAnonymous(Permissions.AppRulesEvents)]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteEvents(string app)
        {
            await ruleEventsRepository.CancelByAppAsync(App.Id, HttpContext.RequestAborted);

            return NoContent();
        }

        /// <summary>
        /// Cancels an event.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The event to enqueue.</param>
        /// <returns>
        /// 204 => Rule deqeued.
        /// 404 => App or rule event not found.
        /// </returns>
        [HttpDelete]
        [Route("apps/{app}/rules/events/{id}/")]
        [ApiPermissionOrAnonymous(Permissions.AppRulesEvents)]
        [ApiCosts(0)]
        public async Task<IActionResult> DeleteEvent(string app, DomainId id)
        {
            var ruleEvent = await ruleEventsRepository.FindAsync(id, HttpContext.RequestAborted);

            if (ruleEvent == null)
            {
                return NotFound();
            }

            await ruleEventsRepository.CancelByRuleAsync(id, HttpContext.RequestAborted);

            return NoContent();
        }

        /// <summary>
        /// Provide a list of all event types that are used in rules.
        /// </summary>
        /// <returns>
        /// 200 => Rule events returned.
        /// </returns>
        [HttpGet]
        [Route("rules/eventtypes")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        [AllowAnonymous]
        public IActionResult GetEventTypes()
        {
            var types = eventJsonSchemaGenerator.AllTypes;

            return Ok(types);
        }

        /// <summary>
        /// Provide the json schema for the event with the specified name.
        /// </summary>
        /// <param name="type">The type name of the event.</param>
        /// <returns>
        /// 200 => Rule event type found.
        /// 404 => Rule event not found.
        /// </returns>
        [HttpGet]
        [Route("rules/eventtypes/{type}")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [AllowAnonymous]
        public IActionResult GetEventSchema(string type)
        {
            var schema = eventJsonSchemaGenerator.GetSchema(type);

            if (schema == null)
            {
                return NotFound();
            }

            return Content(schema.ToJson(), "application/json");
        }

        private async Task<RuleDto> InvokeCommandAsync(ICommand command)
        {
            var context = await CommandBus.PublishAsync(command);

            var runningRuleId = await ruleRunnerService.GetRunningRuleIdAsync(Context.App.Id, HttpContext.RequestAborted);

            var result = context.Result<IEnrichedRuleEntity>();
            var response = RuleDto.FromRule(result, runningRuleId == null, ruleRunnerService, Resources);

            return response;
        }
    }
}
