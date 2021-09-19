// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Management;
using Squidex.Domain.Apps.Core.HandleRules;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;

namespace Squidex.Extensions.Actions.SignalR
{
    public sealed class SignalRActionHandler : RuleActionHandler<SignalRAction, SignalRJob>
    {
        private readonly ClientPool<(string ConnectionString, string HubName), IServiceManager> clients;

        public SignalRActionHandler(RuleEventFormatter formatter)
            : base(formatter)
        {
            clients = new ClientPool<(string ConnectionString, string HubName), IServiceManager>(key =>
            {
                var serviceManager = new ServiceManagerBuilder()
                    .WithOptions(option =>
                    {
                        option.ConnectionString = key.ConnectionString;
                        option.ServiceTransportType = ServiceTransportType.Transient;
                    })
                    .Build();

                return serviceManager;
            });
        }

        protected override async Task<(string Description, SignalRJob Data)> CreateJobAsync(EnrichedEvent @event, SignalRAction action)
        {
            var hubName = await FormatAsync(action.HubName, @event);

            string requestBody;

            if (!string.IsNullOrWhiteSpace(action.Payload))
            {
                requestBody = await FormatAsync(action.Payload, @event);
            }
            else
            {
                requestBody = ToEnvelopeJson(@event);
            }

            var target = (await FormatAsync(action.Target, @event)) ?? string.Empty;

            var ruleDescription = $"Send SignalRJob to signalR hub '{hubName}'";

            var ruleJob = new SignalRJob
            {
                Action = action.Action,
                ConnectionString = action.ConnectionString,
                HubName = hubName,
                MethodName = action.MethodName,
                MethodPayload = requestBody,
                Targets = target.Split("\n")
            };

            return (ruleDescription, ruleJob);
        }

        protected override async Task<Result> ExecuteJobAsync(SignalRJob job,
            CancellationToken ct = default)
        {
            var signalR = await clients.GetClientAsync((job.ConnectionString, job.HubName));

            await using (var signalRContext = await signalR.CreateHubContextAsync(job.HubName, cancellationToken: ct))
            {
                var methodeName = !string.IsNullOrWhiteSpace(job.MethodName) ? job.MethodName : "push";

                switch (job.Action)
                {
                    case ActionTypeEnum.Broadcast:
                        await signalRContext.Clients.All.SendAsync(methodeName, job.MethodPayload, ct);
                        break;
                    case ActionTypeEnum.User:
                        await signalRContext.Clients.Users(job.Targets).SendAsync(methodeName, job.MethodPayload, ct);
                        break;
                    case ActionTypeEnum.Group:
                        await signalRContext.Clients.Groups(job.Targets).SendAsync(methodeName, job.MethodPayload, ct);
                        break;
                }
            }

            return Result.Complete();
        }
    }

    public sealed class SignalRJob
    {
        public string ConnectionString { get; set; }

        public string HubName { get; set; }

        public ActionTypeEnum Action { get; set; }

        public string MethodName { get; set; }

        public string MethodPayload { get; set; }

        public string[] Targets { get; set; }
    }
}
