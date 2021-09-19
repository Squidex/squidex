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
using Squidex.Infrastructure.Translations;
using Squidex.Log;
using TaskExtensions = Squidex.Infrastructure.Tasks.TaskExtensions;

#pragma warning disable RECS0015 // If an extension method is called as static method convert it to method syntax

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

            public bool RunFromSnapshots { get; set; }
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
                RunFromSnapshots = fromSnapshots
            };

            await EnsureIsRunningAsync(false);

            await state.WriteAsync();
        }

        private async Task EnsureIsRunningAsync(bool continues)
        {
            var job = state.Value;

            if (job.RuleId != null && currentJobToken == null)
            {
                if (state.Value.RunFromSnapshots && continues)
                {
                    state.Value = new State();

                    await state.WriteAsync();
                }
                else
                {
                    currentJobToken = new CancellationTokenSource();

#pragma warning disable MA0042 // Do not use blocking calls in an async method
                    Process(state.Value, currentJobToken.Token);
#pragma warning restore MA0042 // Do not use blocking calls in an async method
                }
            }
        }

        private void Process(State job,
            CancellationToken ct)
        {
            TaskExtensions.Forget(ProcessAsync(job, ct));
        }

        private async Task ProcessAsync(State currentState,
            CancellationToken ct)
        {
            try
            {
                currentReminder = await RegisterOrUpdateReminder("KeepAlive", TimeSpan.Zero, TimeSpan.FromMinutes(2));

                var rule = await appProvider.GetRuleAsync(DomainId.Create(Key), currentState.RuleId!.Value, ct);

                if (rule == null)
                {
                    throw new DomainObjectNotFoundException(currentState.RuleId.ToString()!);
                }

                using (localCache.StartContext())
                {
                    var context = new RuleContext
                    {
                        AppId = rule.AppId,
                        Rule = rule.RuleDef,
                        RuleId = rule.Id,
                        IncludeStale = true
                    };

                    if (currentState.RunFromSnapshots && ruleService.CanCreateSnapshotEvents(context))
                    {
                        await EnqueueFromSnapshotsAsync(context, ct);
                    }
                    else
                    {
                        await EnqueueFromEventsAsync(currentState, context, ct);
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

        private async Task EnqueueFromSnapshotsAsync(RuleContext context,
            CancellationToken ct)
        {
            var errors = 0;

            await foreach (var job in ruleService.CreateSnapshotJobsAsync(context, ct))
            {
                if (job.Job != null && job.SkipReason == SkipReason.None)
                {
                    await ruleEventRepository.EnqueueAsync(job.Job, job.EnrichmentError, ct);
                }
                else if (job.EnrichmentError != null)
                {
                    errors++;

                    if (errors >= MaxErrors)
                    {
                        throw job.EnrichmentError;
                    }

                    log.LogWarning(job.EnrichmentError, w => w
                        .WriteProperty("action", "runRule")
                        .WriteProperty("status", "failedPartially"));
                }
            }
        }

        private async Task EnqueueFromEventsAsync(State currentState, RuleContext context,
            CancellationToken ct)
        {
            var errors = 0;

            var filter = $"^([a-z]+)\\-{Key}";

            await foreach (var storedEvent in eventStore.QueryAllAsync(filter, currentState.Position, ct: ct))
            {
                try
                {
                    var @event = eventDataFormatter.ParseIfKnown(storedEvent);

                    if (@event != null)
                    {
                        var jobs = ruleService.CreateJobsAsync(@event, context, ct);

                        await foreach (var job in jobs.WithCancellation(ct))
                        {
                            if (job.Job != null && job.SkipReason == SkipReason.None)
                            {
                                await ruleEventRepository.EnqueueAsync(job.Job, job.EnrichmentError, ct);
                            }
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
            }
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            return EnsureIsRunningAsync(true);
        }
    }
}
