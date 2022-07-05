// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Infrastructure;
using Squidex.Messaging;

namespace Squidex.Domain.Apps.Entities.Rules.Runner
{
    public sealed class RuleRunnerWorker :
        IMessageHandler<RuleRunnerRun>,
        IMessageHandler<RuleRunnerCancel>
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

        public async Task HandleAsync(RuleRunnerRun message,
            CancellationToken ct = default)
        {
            var runner = await GetProcessorAsync(message.AppId);

            await runner.RunAsync(message.RuleId, message.FromSnapshots, false, ct);
        }

        public async Task HandleAsync(RuleRunnerCancel message,
            CancellationToken ct = default)
        {
            var runner = await GetProcessorAsync(message.AppId);

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
