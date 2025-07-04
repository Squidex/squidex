﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Squidex.Areas.Api.Controllers.Rules.Models;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Rules.Commands;
using Squidex.Domain.Apps.Entities.Rules.Runner;
using Squidex.Flows;
using Squidex.Flows.CronJobs;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Security;
using Squidex.Infrastructure.Validation;
using Squidex.Shared;
using Squidex.Web;

namespace Squidex.Areas.Api.Controllers.Rules;

/// <summary>
/// Update and query information about rules.
/// </summary>
[ApiExplorerSettings(GroupName = nameof(Rules))]
public sealed class RulesController(
    ICommandBus commandBus,
    ICronJobManager<CronJobContext> cronJobs,
    IAppProvider appProvider,
    IFlowStepRegistry flowStepRegistry,
    IFlowManager<FlowEventContext> flowManager,
    IRuleQueryService ruleQuery,
    IRuleRunnerService ruleRunnerService,
    IRuleValidator ruleValidator,
    ScriptingCompleter scriptingCompleter,
    EventJsonSchemaGenerator eventJsonSchemaGenerator)
    : ApiController(commandBus)
{
    /// <summary>
    /// Get supported rule steps.
    /// </summary>
    /// <response code="200">Rule actions returned.</response>
    [HttpGet]
    [Route("rules/steps/")]
    [ProducesResponseType(typeof(Dictionary<string, RuleElementDto>), StatusCodes.Status200OK)]
    [ApiPermission]
    [ApiCosts(0)]
    public IActionResult GetSteps()
    {
        var etag = string.Concat(flowStepRegistry.Steps.Select(x => x.Key)).ToSha256Base64();

        var response = Deferred.Response(() =>
        {
            return flowStepRegistry.Steps.ToDictionary(x => x.Key, x => RuleElementDto.FromDomain(x.Value));
        });

        Response.Headers[HeaderNames.ETag] = etag;

        return Ok(response);
    }

    /// <summary>
    /// Get rules.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <response code="200">Rules returned.</response>
    /// <response code="404">App not found.</response>
    [HttpGet]
    [Route("apps/{app}/rules/")]
    [ProducesResponseType(typeof(RulesDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppRulesRead)]
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
    /// <response code="201">Rule created.</response>
    /// <response code="400">Rule request not valid.</response>
    /// <response code="404">App not found.</response>
    [HttpPost]
    [Route("apps/{app}/rules/")]
    [ProducesResponseType(typeof(RuleDto), StatusCodes.Status201Created)]
    [ApiPermissionOrAnonymous(PermissionIds.AppRulesCreate)]
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
    /// <response code="204">Rule run cancelled.</response>
    [HttpDelete]
    [Route("apps/{app}/rules/run")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermissionOrAnonymous(PermissionIds.AppRulesEventsUpdate)]
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
    /// <param name="id">The ID of the rule to update.</param>
    /// <param name="request">The rule object that needs to be added to the app.</param>
    /// <response code="200">Rule updated.</response>
    /// <response code="400">Rule request not valid.</response>
    /// <response code="404">Rule or app not found.</response>
    [HttpPut]
    [Route("apps/{app}/rules/{id}/")]
    [ProducesResponseType(typeof(RuleDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppRulesUpdate)]
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
    /// <param name="id">The ID of the rule to enable.</param>
    /// <response code="200">Rule enabled.</response>
    /// <response code="404">Rule or app not found.</response>
    [HttpPut]
    [Route("apps/{app}/rules/{id}/enable/")]
    [ProducesResponseType(typeof(RuleDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppRulesDisable)]
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
    /// <param name="id">The ID of the rule to disable.</param>
    /// <response code="200">Rule disabled.</response>
    /// <response code="404">Rule or app not found.</response>
    [HttpPut]
    [Route("apps/{app}/rules/{id}/disable/")]
    [ProducesResponseType(typeof(RuleDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppRulesDisable)]
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
    /// <param name="id">The ID of the rule to disable.</param>
    /// <param name="request">The arguments for the rule flow.</param>
    /// <response code="204">Rule triggered.</response>
    /// <response code="404">Rule or app not found.</response>
    [HttpPut]
    [Route("apps/{app}/rules/{id}/trigger/")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermissionOrAnonymous(PermissionIds.AppRulesEventsRun)]
    [ApiCosts(1)]
    public async Task<IActionResult> TriggerRule(string app, DomainId id, [FromBody] TriggerRuleDto request)
    {
        var command = request.ToCommand(id);

        await CommandBus.PublishAsync(command, HttpContext.RequestAborted);

        return NoContent();
    }

    /// <summary>
    /// Run a rule.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="id">The ID of the rule to run.</param>
    /// <param name="fromSnapshots">Runs the rule from snapeshots if possible.</param>
    /// <response code="204">Rule started.</response>
    [HttpPut]
    [Route("apps/{app}/rules/{id}/run")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermissionOrAnonymous(PermissionIds.AppRulesEventsRun)]
    [ApiCosts(1)]
    public async Task<IActionResult> PutRuleRun(string app, DomainId id, [FromQuery] bool fromSnapshots = false)
    {
        await ruleRunnerService.RunAsync(User.Token()!, App, id, fromSnapshots, HttpContext.RequestAborted);

        return NoContent();
    }

    /// <summary>
    /// Cancels all rule events.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="id">The ID of the rule to cancel.</param>
    /// <response code="204">Rule events cancelled.</response>
    [HttpDelete]
    [Route("apps/{app}/rules/{id}/events/")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermissionOrAnonymous(PermissionIds.AppRulesEventsDelete)]
    [ApiCosts(1)]
    public async Task<IActionResult> DeleteRuleEvents(string app, DomainId id)
    {
        await flowManager.CancelByDefinitionIdAsync(id.ToString(), HttpContext.RequestAborted);

        return NoContent();
    }

    /// <summary>
    /// Validates a rule trigger.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="request">The rule trigger that needs to be validate.</param>
    /// <response code="204">Rule trigger validated.</response>
    /// <response code="400">Rule trigger not valid.</response>
    /// <response code="404">Rule or app not found.</response>
    [HttpPost]
    [Route("apps/{app}/rules/validate/trigger/", Order = -1)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermission]
    [ApiCosts(0)]
    public async Task<IActionResult> ValidateTrigger(string app, [FromBody] RuleTriggerDto request)
    {
        await Validate.It(async e =>
        {
            await ruleValidator.ValidateTriggerAsync(request.ToTrigger(), AppId, e,
                HttpContext.RequestAborted);
        });

        return NoContent();
    }

    /// <summary>
    /// Validates a rule step.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="request">The rule step that needs to be validate.</param>
    /// <response code="204">Rule step validated.</response>
    /// <response code="400">Rule step not valid.</response>
    /// <response code="404">Rule or app not found.</response>
    [HttpPost]
    [Route("apps/{app}/rules/validate/step/", Order = -1)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermission]
    [ApiCosts(0)]
    public async Task<IActionResult> ValidateStep(string app, [FromBody] FlowStep request)
    {
        await Validate.It(async e =>
        {
            await ruleValidator.ValidateStepAsync(request, e,
                HttpContext.RequestAborted);
        });

        return NoContent();
    }

    /// <summary>
    /// Simulate a rule.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="request">The rule to simulate.</param>
    /// <response code="200">Rule simulated.</response>
    /// <response code="404">Rule or app not found.</response>
    [HttpPost]
    [Route("apps/{app}/rules/simulate/")]
    [ProducesResponseType(typeof(SimulatedRuleEventsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppRulesEventsRead)]
    [ApiCosts(5)]
    public async Task<IActionResult> Simulate(string app, [FromBody] CreateRuleDto request)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var flow = request.GetFlow();
#pragma warning restore CS0618 // Type or member is obsolete

        if (flow == null)
        {
            return Ok(SimulatedRuleEventsDto.FromDomain([]));
        }

        var simulation =
            await ruleRunnerService.SimulateAsync(App.NamedId(), request.Trigger.ToTrigger(), flow,
                HttpContext.RequestAborted);

        var response = SimulatedRuleEventsDto.FromDomain(simulation);

        return Ok(response);
    }

    /// <summary>
    /// Simulate a rule.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="id">The ID of the rule to simulate.</param>
    /// <response code="200">Rule simulated.</response>
    /// <response code="404">Rule or app not found.</response>
    [HttpGet]
    [Route("apps/{app}/rules/{id}/simulate/")]
    [ProducesResponseType(typeof(SimulatedRuleEventsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppRulesEventsRead)]
    [ApiCosts(5)]
    public async Task<IActionResult> Simulate(string app, DomainId id)
    {
        var rule = await appProvider.GetRuleAsync(AppId, id, HttpContext.RequestAborted);

        if (rule == null)
        {
            return NotFound();
        }

        var result = await ruleRunnerService.SimulateAsync(rule, HttpContext.RequestAborted);
        var response = SimulatedRuleEventsDto.FromDomain(result);

        return Ok(response);
    }

    /// <summary>
    /// Delete a rule.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="id">The ID of the rule to delete.</param>
    /// <response code="204">Rule deleted.</response>
    /// <response code="404">Rule or app not found.</response>
    [HttpDelete]
    [Route("apps/{app}/rules/{id}/")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermissionOrAnonymous(PermissionIds.AppRulesDelete)]
    [ApiCosts(1)]
    public async Task<IActionResult> DeleteRule(string app, DomainId id)
    {
        var command = new DeleteRule { RuleId = id };

        await CommandBus.PublishAsync(command, HttpContext.RequestAborted);

        return NoContent();
    }

    /// <summary>
    /// Get rule events.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="ruleId">The optional rule id to filter to events.</param>
    /// <param name="skip">The number of events to skip.</param>
    /// <param name="take">The number of events to take.</param>
    /// <response code="200">Rule events returned.</response>
    /// <response code="404">App not found.</response>
    [HttpGet]
    [Route("apps/{app}/rules/events/")]
    [ProducesResponseType(typeof(RuleEventsDto), StatusCodes.Status200OK)]
    [ApiPermissionOrAnonymous(PermissionIds.AppRulesEventsRead)]
    [ApiCosts(0)]
    public async Task<IActionResult> GetEvents(string app, [FromQuery] DomainId? ruleId = null, [FromQuery] int skip = 0, [FromQuery] int take = 20)
    {
        var (states, total) =
            await flowManager.QueryInstancesByOwnerAsync(AppId.ToString(), ruleId?.ToString(), skip, take,
                HttpContext.RequestAborted);

        var response = RuleEventsDto.FromDomain(ResultList.Create(total, states), Resources, ruleId);

        return Ok(response);
    }

    /// <summary>
    /// Retry the event immediately.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="id">The event to enqueue.</param>
    /// <response code="204">Rule enqueued.</response>
    /// <response code="404">App or rule event not found.</response>
    [HttpPut]
    [Route("apps/{app}/rules/events/{id}/")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermissionOrAnonymous(PermissionIds.AppRulesEventsUpdate)]
    [ApiCosts(0)]
    public async Task<IActionResult> PutEvent(string app, Guid id)
    {
        if (!await flowManager.ForceAsync(id, HttpContext.RequestAborted))
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Cancels an event.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <param name="id">The event to cancel.</param>
    /// <response code="204">Rule event cancelled.</response>
    /// <response code="404">App or rule event not found.</response>
    [HttpDelete]
    [Route("apps/{app}/rules/events/{id}/")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermission]
    [ApiCosts(0)]
    public async Task<IActionResult> DeleteEvent(string app, Guid id)
    {
        if (!await flowManager.CancelByInstanceIdAsync(id, HttpContext.RequestAborted))
        {
            return NotFound();
        }

        return NoContent();
    }

    /// <summary>
    /// Cancels all events.
    /// </summary>
    /// <param name="app">The name of the app.</param>
    /// <response code="204">Events cancelled.</response>
    [HttpDelete]
    [Route("apps/{app}/rules/events/")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ApiPermissionOrAnonymous(PermissionIds.AppRulesEventsDelete)]
    [ApiCosts(1)]
    public async Task<IActionResult> DeleteEvents(string app)
    {
        await flowManager.CancelByOwnerIdAsync(App.Id.ToString(), HttpContext.RequestAborted);
        return NoContent();
    }

    /// <summary>
    /// Provide a list of all event types that are used in rules.
    /// </summary>
    /// <response code="200">Rule events returned.</response>
    [HttpGet]
    [Route("rules/eventtypes")]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
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
    /// <response code="200">Rule event type found.</response>
    /// <response code="404">Rule event not found.</response>
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

    [HttpGet]
    [Route("apps/{app}/rules/completion/{triggerType}")]
    [ApiPermissionOrAnonymous]
    [ApiCosts(1)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult GetScriptCompletion(string app, string triggerType)
    {
        var completion = scriptingCompleter.Trigger(triggerType);

        return Ok(completion);
    }

    [HttpGet]
    [Route("apps/{app}/rules/timezones")]
    [ApiPermissionOrAnonymous]
    [ApiCosts(1)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult GetTimezones(string app)
    {
        var timezones = cronJobs.GetAvailableTimezoneIds();

        return Ok(timezones);
    }

    private async Task<RuleDto> InvokeCommandAsync(ICommand command)
    {
        var context = await CommandBus.PublishAsync(command, HttpContext.RequestAborted);

        var runningRuleId = await ruleRunnerService.GetRunningRuleIdAsync(Context.App.Id, HttpContext.RequestAborted);

        var result = context.Result<EnrichedRule>();
        var response = RuleDto.FromDomain(result, runningRuleId == null, ruleRunnerService, Resources);

        return response;
    }
}
