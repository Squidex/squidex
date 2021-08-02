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

            var ruleDescription = $"Send SignalRJob to signalR hub '{hubName}'";

            var ruleJob = new SignalRJob
            {
                ConnectionString = action.ConnectionString,
                HubName = hubName,
                Action = action.ActionType,
                MethodName = action.MethodName,
                Group = await FormatAsync(action.Group, @event),
                User = await FormatAsync(action.User, @event),
                Payload = requestBody
            };
            return (ruleDescription, ruleJob);
        }

        protected override async Task<Result> ExecuteJobAsync(SignalRJob job, CancellationToken ct = default)
        {
            var signalR = await clients.GetClientAsync((job.ConnectionString, job.HubName));
            var signalRContext = await signalR.CreateHubContextAsync(job.HubName);

            var methodeName = !string.IsNullOrWhiteSpace(job.MethodName) ? job.MethodName : "push";

            switch (job.Action)
            {
                case ActionTypeEnum.BROADCAST:
                    await signalRContext.Clients.All.SendAsync(methodeName, job.Payload);
                    break;
                case ActionTypeEnum.USER:
                    await signalRContext.Clients.User(job.User).SendAsync(methodeName, job.Payload);
                    break;
                case ActionTypeEnum.USERS:
                    var userIds = job.User.Split('\n');
                    await signalRContext.Clients.Users(userIds).SendAsync(methodeName, job.Payload);
                    break;
                case ActionTypeEnum.GROUP:
                    await signalRContext.Clients.Group(job.Group).SendAsync(methodeName, job.Payload);
                    break;
                case ActionTypeEnum.GROUPS:
                    var groupIds = job.Group.Split('\n');
                    await signalRContext.Clients.Groups(groupIds).SendAsync(methodeName, job.Payload);
                    break;
            }

            await signalRContext.DisposeAsync();

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
        public string Group { get; set; }
        public string Payload { get; set; }
    }
}
