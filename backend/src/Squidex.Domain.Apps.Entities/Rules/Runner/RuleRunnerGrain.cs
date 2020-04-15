// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
using System.Threading.Tasks;
using Orleans;
using Orleans.Runtime;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Entities.Rules.Runner
{
    public sealed class RuleRunnerGrain : GrainOfGuid, IRuleRunnerGrain, IRemindable
    {
        private readonly IGrainState<State> state;
        private readonly IAppProvider appProvider;
        private readonly IEventStore eventStore;
        private readonly IEventDataFormatter eventDataFormatter;
        private readonly IRuleEventRepository ruleEventRepository;
        private readonly RuleService ruleService;
        private readonly ISemanticLog log;
        private CancellationTokenSource? currentTaskToken;
        private IGrainReminder? currentReminder;
        private bool isStopping;

        [CollectionName("Rules_Runner")]
        public sealed class State
        {
            public Guid? RuleId { get; set; }

            public string? Position { get; set; }
        }

        public RuleRunnerGrain(
            IGrainState<State> state,
            IAppProvider appProvider,
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            IRuleEventRepository ruleEventRepository,
            RuleService ruleService,
            ISemanticLog log)
        {
            Guard.NotNull(state);
            Guard.NotNull(appProvider);
            Guard.NotNull(eventStore);
            Guard.NotNull(eventDataFormatter);
            Guard.NotNull(ruleEventRepository);
            Guard.NotNull(ruleService);
            Guard.NotNull(log);

            this.state = state;
            this.appProvider = appProvider;
            this.eventStore = eventStore;
            this.eventDataFormatter = eventDataFormatter;
            this.ruleEventRepository = ruleEventRepository;
            this.ruleService = ruleService;
            this.log = log;
        }

        protected override Task OnActivateAsync(Guid key)
        {
            EnsureIsRunning();

            return base.OnActivateAsync(key);
        }

        public override Task OnDeactivateAsync()
        {
            isStopping = true;

            currentTaskToken?.Cancel();

            return base.OnDeactivateAsync();
        }

        public Task CancelAsync()
        {
            currentTaskToken?.Cancel();

            return Task.CompletedTask;
        }

        public Task<Guid?> GetRunningRuleIdAsync()
        {
            return Task.FromResult(state.Value.RuleId);
        }

        public async Task RunAsync(Guid ruleId)
        {
            if (currentTaskToken != null)
            {
                throw new DomainException("Another rule is already running.");
            }

            state.Value = new State
            {
                RuleId = ruleId
            };

            EnsureIsRunning();

            await state.WriteAsync();
        }

        private void EnsureIsRunning()
        {
            if (state.Value.RuleId.HasValue && currentTaskToken == null)
            {
                currentTaskToken = new CancellationTokenSource();

                Process(state.Value, currentTaskToken.Token);
            }
        }

        private void Process(State job, CancellationToken ct)
        {
            ProcessAsync(job, ct).Forget();
        }

        private async Task ProcessAsync(State job, CancellationToken ct)
        {
            try
            {
                currentReminder = await RegisterOrUpdateReminder("KeepAlive", TimeSpan.Zero, TimeSpan.FromMinutes(2));

                var rules = await appProvider.GetRulesAsync(Key);

                var rule = rules.Find(x => x.Id == job.RuleId);

                if (rule == null)
                {
                    throw new InvalidOperationException("Cannot find rule.");
                }

                await eventStore.QueryAsync(async storedEvent =>
                {
                    var @event = eventDataFormatter.Parse(storedEvent.Data);

                    var jobs = await ruleService.CreateJobsAsync(rule.RuleDef, rule.Id, @event);

                    foreach (var job in jobs)
                    {
                        await ruleEventRepository.EnqueueAsync(job, job.Created, ct);
                    }

                    job.Position = storedEvent.EventPosition;

                    await state.WriteAsync();
                }, SquidexHeaders.AppId, Key.ToString(), job.Position, ct);
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "runeRule")
                    .WriteProperty("status", "failed")
                    .WriteProperty("ruleId", job.RuleId?.ToString()));
            }
            finally
            {
                if (!isStopping)
                {
                    job.RuleId = null;
                    job.Position = null;

                    await state.WriteAsync();

                    if (currentReminder != null)
                    {
                        await UnregisterReminder(currentReminder);

                        currentReminder = null;
                    }

                    currentTaskToken = null;
                }
            }
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            EnsureIsRunning();

            return Task.CompletedTask;
        }
    }
}
