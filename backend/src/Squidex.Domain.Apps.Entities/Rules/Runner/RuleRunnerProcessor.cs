// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Squidex.Caching;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Entities.Rules.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Entities.Rules.Runner
{
    public sealed class RuleRunnerProcessor
    {
        private const int MaxErrors = 10;
        private readonly IAppProvider appProvider;
        private readonly IEventFormatter eventFormatter;
        private readonly IEventStore eventStore;
        private readonly ILocalCache localCache;
        private readonly IRuleEventRepository ruleEventRepository;
        private readonly IRuleService ruleService;
        private readonly ILogger<RuleRunnerProcessor> log;
        private readonly SimpleState<RuleRunnerState> state;
        private readonly DomainId appId;
        private CancellationTokenSource? currentJobToken;
        private bool isStopping;

        public RuleRunnerProcessor(
            DomainId appId,
            IAppProvider appProvider,
            IEventFormatter eventFormatter,
            IEventStore eventStore,
            ILocalCache localCache,
            IPersistenceFactory<RuleRunnerState> persistenceFactory,
            IRuleEventRepository ruleEventRepository,
            IRuleService ruleService,
            ILogger<RuleRunnerProcessor> log)
        {
            this.appId = appId;
            this.appProvider = appProvider;
            this.localCache = localCache;
            this.eventStore = eventStore;
            this.eventFormatter = eventFormatter;
            this.ruleEventRepository = ruleEventRepository;
            this.ruleService = ruleService;
            this.log = log;

            state = new SimpleState<RuleRunnerState>(persistenceFactory, GetType(), appId);
        }

        public Task LoadAsync(
            CancellationToken ct = default)
        {
            return state.LoadAsync(ct);
        }

        public Task ReleaseAsync()
        {
            isStopping = true;

            currentJobToken?.Cancel();

            return Task.CompletedTask;
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

        public async Task RunAsync(DomainId ruleId, bool fromSnapshots, bool isRetry,
            CancellationToken ct)
        {
            if (currentJobToken != null)
            {
                throw new DomainException(T.Get("rules.ruleAlreadyRunning"));
            }

            state.Value = new RuleRunnerState
            {
                RuleId = ruleId,
                RunFromSnapshots = fromSnapshots
            };

            await state.WriteAsync(ct);

            await EnsureIsRunningAsync(isRetry, ct);
        }

        private async Task EnsureIsRunningAsync(bool isRetry,
            CancellationToken ct)
        {
            var job = state.Value;

            if (job.RuleId != null && currentJobToken == null)
            {
                if (state.Value.RunFromSnapshots && isRetry)
                {
                    state.Value = new RuleRunnerState();

                    await state.WriteAsync(ct);
                }
                else
                {
                    currentJobToken = new CancellationTokenSource();

                    using (var linked = CancellationTokenSource.CreateLinkedTokenSource(currentJobToken.Token, ct))
                    {
                        await ProcessAsync(state.Value, ct);
                    }
                }
            }
        }

        private async Task ProcessAsync(RuleRunnerState currentState,
            CancellationToken ct)
        {
            try
            {
                var rule = await appProvider.GetRuleAsync(appId, currentState.RuleId!.Value, ct);

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
                log.LogError(ex, "Failed to run rule with ID {ruleId}.", currentState.RuleId);
            }
            finally
            {
                if (!isStopping)
                {
                    currentState.RuleId = null;
                    currentState.Position = null;

                    await state.WriteAsync(ct);

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

                    log.LogWarning(job.EnrichmentError, "Failed to run rule with ID {ruleId}, continue with next job.", context.RuleId);
                }
            }
        }

        private async Task EnqueueFromEventsAsync(RuleRunnerState currentState, RuleContext context,
            CancellationToken ct)
        {
            var errors = 0;

            var filter = $"^([a-z]+)\\-{appId}";

            await foreach (var storedEvent in eventStore.QueryAllAsync(filter, currentState.Position, ct: ct))
            {
                try
                {
                    var @event = eventFormatter.ParseIfKnown(storedEvent);

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

                    log.LogWarning(ex, "Failed to run rule with ID {ruleId}, continue with next job.", context.RuleId);
                }
                finally
                {
                    currentState.Position = storedEvent.EventPosition;
                }

                await state.WriteAsync(ct);
            }
        }
    }
}
