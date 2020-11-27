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
using Squidex.Caching;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.Translations;
using Squidex.Log;

namespace Squidex.Domain.Apps.Entities.Rules.Runner
{
    public sealed class RuleRunnerGrain : GrainOfString, IRuleRunnerGrain, IRemindable
    {
        private const int MaxErrors = 10;
        private readonly IGrainState<State> state;
        private readonly IAppProvider appProvider;
        private readonly ILocalCache localCache;
        private readonly IEventStore eventStore;
        private readonly IEventDataFormatter eventDataFormatter;
        private readonly IRuleEventRepository ruleEventRepository;
        private readonly IRuleService ruleService;
        private readonly ISemanticLog log;
        private CancellationTokenSource? currentJobToken;
        private IGrainReminder? currentReminder;
        private bool isStopping;

        [CollectionName("Rules_Runner")]
        public sealed class State
        {
            public DomainId? RuleId { get; set; }

            public string? Position { get; set; }

            public bool FromSnapshots { get; set; }
        }

        public RuleRunnerGrain(
            IGrainState<State> state,
            IAppProvider appProvider,
            ILocalCache localCache,
            IEventStore eventStore,
            IEventDataFormatter eventDataFormatter,
            IRuleEventRepository ruleEventRepository,
            IRuleService ruleService,
            ISemanticLog log)
        {
            Guard.NotNull(state, nameof(state));
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(localCache, nameof(localCache));
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(eventDataFormatter, nameof(eventDataFormatter));
            Guard.NotNull(ruleEventRepository, nameof(ruleEventRepository));
            Guard.NotNull(ruleService, nameof(ruleService));
            Guard.NotNull(log, nameof(log));

            this.state = state;
            this.appProvider = appProvider;
            this.localCache = localCache;
            this.eventStore = eventStore;
            this.eventDataFormatter = eventDataFormatter;
            this.ruleEventRepository = ruleEventRepository;
            this.ruleService = ruleService;
            this.log = log;
        }

        protected override Task OnActivateAsync(string key)
        {
            return EnsureIsRunningAsync(true);
        }

        public override Task OnDeactivateAsync()
        {
            isStopping = true;

            currentJobToken?.Cancel();

            return base.OnDeactivateAsync();
        }

        public Task CancelAsync()
        {
            try
            {
                currentJobToken?.Cancel();
            }
            catch (ObjectDisposedException)
            {
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        public Task<DomainId?> GetRunningRuleIdAsync()
        {
            return Task.FromResult(state.Value.RuleId);
        }

        public async Task RunAsync(DomainId ruleId, bool fromSnapshots)
        {
            if (currentJobToken != null)
            {
                throw new DomainException(T.Get("rules.ruleAlreadyRunning"));
            }

            state.Value = new State
            {
                RuleId = ruleId,
                FromSnapshots = fromSnapshots
            };

            await EnsureIsRunningAsync(false);

            await state.WriteAsync();
        }

        private async Task EnsureIsRunningAsync(bool continues)
        {
            var job = state.Value;

            if (job.RuleId.HasValue && currentJobToken == null)
            {
                if (state.Value.FromSnapshots && continues)
                {
                    state.Value = new State();

                    await state.WriteAsync();
                }
                else
                {
                    currentJobToken = new CancellationTokenSource();

                    Process(state.Value, currentJobToken.Token);
                }
            }
        }

        private void Process(State job, CancellationToken ct)
        {
            ProcessAsync(job, ct).Forget();
        }

        private async Task ProcessAsync(State currentState, CancellationToken ct)
        {
            try
            {
                currentReminder = await RegisterOrUpdateReminder("KeepAlive", TimeSpan.Zero, TimeSpan.FromMinutes(2));

                var rules = await appProvider.GetRulesAsync(DomainId.Create(Key));

                var rule = rules.Find(x => x.Id == currentState.RuleId);

                if (rule == null)
                {
                    throw new InvalidOperationException("Cannot find rule.");
                }

                using (localCache.StartContext())
                {
                    if (currentState.FromSnapshots && ruleService.CanCreateSnapshotEvents(rule.RuleDef))
                    {
                        await EnqueueFromSnapshotsAsync(rule);
                    }
                    else
                    {
                        await EnqueueFromEventsAsync(currentState, rule, ct);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                return;
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "runRule")
                    .WriteProperty("status", "failed")
                    .WriteProperty("ruleId", currentState.RuleId?.ToString()));
            }
            finally
            {
                if (!isStopping)
                {
                    currentState.RuleId = null;
                    currentState.Position = null;

                    await state.WriteAsync();

                    if (currentReminder != null)
                    {
                        await UnregisterReminder(currentReminder);

                        currentReminder = null;
                    }

                    currentJobToken?.Dispose();
                    currentJobToken = null;
                }
            }
        }

        private async Task EnqueueFromSnapshotsAsync(IRuleEntity rule)
        {
            var errors = 0;

            await foreach (var (job, ex) in ruleService.CreateSnapshotJobsAsync(rule.RuleDef, rule.Id, rule.AppId.Id))
            {
                if (job != null)
                {
                    await ruleEventRepository.EnqueueAsync(job, ex);
                }
                else if (ex != null)
                {
                    errors++;

                    if (errors >= MaxErrors)
                    {
                        throw ex;
                    }

                    log.LogWarning(ex, w => w
                        .WriteProperty("action", "runRule")
                        .WriteProperty("status", "failedPartially"));
                }
            }
        }

        private async Task EnqueueFromEventsAsync(State currentState, IRuleEntity rule, CancellationToken ct)
        {
            var errors = 0;

            await eventStore.QueryAsync(async storedEvent =>
            {
                try
                {
                    var @event = eventDataFormatter.ParseIfKnown(storedEvent);

                    if (@event != null)
                    {
                        var jobs = await ruleService.CreateJobsAsync(rule.RuleDef, rule.Id, @event, false);

                        foreach (var (job, ex) in jobs)
                        {
                            await ruleEventRepository.EnqueueAsync(job, ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors++;

                    if (errors >= MaxErrors)
                    {
                        throw;
                    }

                    log.LogWarning(ex, w => w
                        .WriteProperty("action", "runRule")
                        .WriteProperty("status", "failedPartially"));
                }
                finally
                {
                    currentState.Position = storedEvent.EventPosition;
                }

                await state.WriteAsync();
            }, $"^([a-z]+)\\-{Key}", currentState.Position, ct);
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            return EnsureIsRunningAsync(true);
        }
    }
}
