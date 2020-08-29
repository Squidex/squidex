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
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Entities.Rules.Runner
{
    public sealed class RuleRunnerGrain : GrainOfString, IRuleRunnerGrain, IRemindable
    {
        private readonly IGrainState<State> state;
        private readonly IAppProvider appProvider;
        private readonly IEventStore eventStore;
        private readonly IEventDataFormatter eventDataFormatter;
        private readonly IRuleEventRepository ruleEventRepository;
        private readonly RuleService ruleService;
        private readonly ISemanticLog log;
        private CancellationTokenSource? currentJobToken;
        private IGrainReminder? currentReminder;
        private bool isStopping;

        [CollectionName("Rules_Runner")]
        public sealed class State
        {
            public DomainId? RuleId { get; set; }

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
            Guard.NotNull(state, nameof(state));
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(eventStore, nameof(eventStore));
            Guard.NotNull(eventDataFormatter, nameof(eventDataFormatter));
            Guard.NotNull(ruleEventRepository, nameof(ruleEventRepository));
            Guard.NotNull(ruleService, nameof(ruleService));
            Guard.NotNull(log, nameof(log));

            this.state = state;
            this.appProvider = appProvider;
            this.eventStore = eventStore;
            this.eventDataFormatter = eventDataFormatter;
            this.ruleEventRepository = ruleEventRepository;
            this.ruleService = ruleService;
            this.log = log;
        }

        protected override Task OnActivateAsync(string key)
        {
            EnsureIsRunning();

            return base.OnActivateAsync(key);
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

        public async Task RunAsync(DomainId ruleId)
        {
            if (currentJobToken != null)
            {
                throw new DomainException(T.Get("rules.ruleAlreadyRunning"));
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
            if (state.Value.RuleId.HasValue && currentJobToken == null)
            {
                currentJobToken = new CancellationTokenSource();

                Process(state.Value, currentJobToken.Token);
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
                    try
                    {
                        var @event = ParseKnownEvent(storedEvent);

                        if (@event != null)
                        {
                            var jobs = await ruleService.CreateJobsAsync(rule.RuleDef, rule.Id, @event, false);

                            foreach (var (job, _) in jobs)
                            {
                                await ruleEventRepository.EnqueueAsync(job, job.Created, ct);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.LogWarning(ex, w => w
                            .WriteProperty("action", "runRule")
                            .WriteProperty("status", "failedPartially3"));
                    }
                    finally
                    {
                        job.Position = storedEvent.EventPosition;
                    }

                    await state.WriteAsync();
                }, $"\\-{Key}", job.Position, ct);
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

                    currentJobToken?.Dispose();
                    currentJobToken = null;
                }
            }
        }

        private Envelope<IEvent>? ParseKnownEvent(StoredEvent storedEvent)
        {
            try
            {
                var @event = eventDataFormatter.Parse(storedEvent.Data);

                @event.SetEventPosition(storedEvent.EventPosition);
                @event.SetEventStreamNumber(storedEvent.EventStreamNumber);

                return @event;
            }
            catch (TypeNameNotFoundException)
            {
                return null;
            }
        }

        public Task ReceiveReminder(string reminderName, TickStatus status)
        {
            EnsureIsRunning();

            return Task.CompletedTask;
        }
    }
}
