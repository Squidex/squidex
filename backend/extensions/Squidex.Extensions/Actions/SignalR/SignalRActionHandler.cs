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

            if (!string.IsNullOrEmpty(action.Payload))
            {
                requestBody = await FormatAsync(action.Payload, @event);
            }
            else
            {
                requestBody = ToEnvelopeJson(@event);
            }

            string[] users = null;
            if (!string.IsNullOrEmpty(action.User) && action.User.IndexOf("\n", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                users = action.User.Split('\n');
            }

            string[] groups = null;
            if (!string.IsNullOrEmpty(action.Group) && action.Group.IndexOf("\n", System.StringComparison.OrdinalIgnoreCase) >= 0)
            {
                groups = action.Group.Split('\n');
            }

            var ruleDescription = $"Send SignalRJob to signalR hub '{hubName}'";

            var ruleJob = new SignalRJob
            {
                ConnectionString = action.ConnectionString,
                HubName = hubName,
                Action = action.ActionType,
                MethodName = action.MethodName,
                User = await FormatAsync(action.User, @event),
                Users = users ?? new string[0],
                Group = await FormatAsync(action.Group, @event),
                Groups = groups ?? new string[0],
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
                        await signalRContext.Clients.User(job.User).SendAsync(methodeName, job.Payload);
                        break;
                    case ActionTypeEnum.Users:
                        await signalRContext.Clients.Users(job.Users).SendAsync(methodeName, job.Payload);
                        break;
                    case ActionTypeEnum.Group:
                        await signalRContext.Clients.Group(job.Group).SendAsync(methodeName, job.Payload);
                        break;
                    case ActionTypeEnum.Groups:
                        await signalRContext.Clients.Groups(job.Groups).SendAsync(methodeName, job.Payload);
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
