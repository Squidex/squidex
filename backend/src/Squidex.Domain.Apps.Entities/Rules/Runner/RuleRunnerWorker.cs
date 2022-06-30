// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Rules.Runner
{
    public sealed class RuleRunnerWorker :
        IJobConsumer<RuleRunnerRun>,
        IConsumer<RuleRunnerCancel>
    {
        private readonly ConcurrentDictionary<DomainId, RuleRunnerProcessor> processors = new ConcurrentDictionary<DomainId, RuleRunnerProcessor>();
        private readonly Func<DomainId, RuleRunnerProcessor> processorFactory;

        public RuleRunnerWorker(IServiceProvider serviceProvider)
        {
            var objectFactory = ActivatorUtilities.CreateFactory(typeof(RuleRunnerProcessor), new[] { typeof(DomainId) });

            processorFactory = key =>
            {
                return (RuleRunnerProcessor)objectFactory(serviceProvider, new object[] { key });
            };
        }

        public async Task Run(JobContext<RuleRunnerRun> context)
        {
            var runner = await GetProcessorAsync(context.Job.AppId);

            await runner.RunAsync(context.Job.RuleId, context.Job.FromSnapshots, context.RetryAttempt > 0, context.CancellationToken);
        }

        public async Task Consume(ConsumeContext<RuleRunnerCancel> context)
        {
            var runner = await GetProcessorAsync(context.Message.AppId);

            await runner.CancelAsync();
        }

        private async Task<RuleRunnerProcessor> GetProcessorAsync(DomainId appId)
        {
            var runner = processors.GetOrAdd(appId, processorFactory);

            await runner.LoadAsync(default);

            return runner;
        }
    }
}
