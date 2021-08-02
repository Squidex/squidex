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

            string[] users = new string[0];
            string user = string.Empty;
            if (!string.IsNullOrWhiteSpace(action.User) && action.User.IndexOf("\n", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                users = action.User.Split('\n');
            }
            else if (!string.IsNullOrWhiteSpace(action.User))
            {
                user = await FormatAsync(action.User, @event);
            }

            string[] groups = new string[0];
            string group = string.Empty;
            if (!string.IsNullOrEmpty(action.Group) && action.Group.IndexOf("\n", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                groups = action.Group.Split('\n');
            }
            else if (!string.IsNullOrWhiteSpace(action.Group))
            {
                group = await FormatAsync(action.Group, @event);
            }

            var ruleDescription = $"Send SignalRJob to signalR hub '{hubName}'";

            var ruleJob = new SignalRJob
            {
                ConnectionString = action.ConnectionString,
                HubName = hubName,
                Action = action.ActionType,
                MethodName = action.MethodName,
                User = user,
                Users = users,
                Group = group,
                Groups = groups,
                Payload = requestBody
            };
            return (ruleDescription, ruleJob);
        }

        protected override async Task<Result> ExecuteJobAsync(SignalRJob job, CancellationToken ct = default)
        {
            var signalR = await clients.GetClientAsync((job.ConnectionString, job.HubName));

            await using (var signalRContext = await signalR.CreateHubContextAsync(job.HubName))
            {
                var methodeName = !string.IsNullOrWhiteSpace(job.MethodName) ? job.MethodName : "push";

                switch (job.Action)
                {
                    case ActionTypeEnum.Broadcast:
                        await signalRContext.Clients.All.SendAsync(methodeName, job.Payload);
                        break;
                    case ActionTypeEnum.User:
                        if (!string.IsNullOrWhiteSpace(job.User))
                        {
                            await signalRContext.Clients.User(job.User).SendAsync(methodeName, job.Payload);
                        }
                        else
                        {
                            await signalRContext.Clients.Users(job.Users).SendAsync(methodeName, job.Payload);
                        }

                        break;
                    case ActionTypeEnum.Group:
                        if (!string.IsNullOrWhiteSpace(job.User))
                        {
                            await signalRContext.Clients.Group(job.Group).SendAsync(methodeName, job.Payload);
                        }
                        else
                        {
                            await signalRContext.Clients.Groups(job.Groups).SendAsync(methodeName, job.Payload);
                        }

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

        public string User { get; set; }

        public string[] Users { get; set; }

        public string Group { get; set; }

        public string[] Groups { get; set; }

        public string Payload { get; set; }
    }
}
