using DeploymentApp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Squidex.ICIS.Kafka.Consumer;
using Squidex.Infrastructure.Log;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Squidex.ICIS.Deployment
{
    public sealed class DeploymentService
    {
        private readonly IEnumerable<IKafkaConsumerService> kafkaConsumers;
        private readonly DeploymentOptions options;
        private readonly IApplicationLifetime lifetime;
        private readonly ISemanticLog log;
        private volatile int started;

        private sealed class Logger : ILogger
        {
            private readonly List<(string Process, string Status)> steps = new List<(string Process, string Status)>();

            public void Start(string process)
            {
                steps.Add((process, "Pending"));
            }

            public void Success()
            {
                Complete("succeeded.");
            }

            public void Skipped(string reason)
            {
                Complete($"skipped: {reason}");
            }

            public void Failed(Exception ex)
            {
                Complete($"failed with {ex.Message}.");
            }

            private void Complete(string result)
            {
                steps[steps.Count - 1] = (steps[steps.Count  - 1].Process, result);
            }

            public void Log(ISemanticLog log)
            {
                log.LogInformation(w => w
                    .WriteProperty("action", "Deployment")
                    .WriteProperty("status", "Completed")
                    .WriteArray("steps", LogSteps));
            }

            public void Log(ISemanticLog log, Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "Deployment")
                    .WriteProperty("status", "Failed")
                    .WriteArray("steps", LogSteps));
            }

            private void LogSteps(IArrayWriter writer)
            {
                foreach (var (action, status) in steps)
                {
                    writer.WriteObject(w => w
                        .WriteProperty("name", action)
                        .WriteProperty("result", status));
                }
            }
        }

        public DeploymentService(IEnumerable<IKafkaConsumerService> kafkaConsumers, IOptions<DeploymentOptions> options, IApplicationLifetime lifetime, ISemanticLog log)
        {
            this.kafkaConsumers = kafkaConsumers;
            this.options = options.Value;
            this.lifetime = lifetime;
            this.log = log;
        }

        public bool Start(bool kafka, bool deploy)
        {
            if (Interlocked.Increment(ref started) != 1)
            {
                return false;
            }

            Task.Run(async () =>
            {
                if (deploy)
                {
                    await RunDeployment();
                }

                if (kafka)
                {
                    StartConsumers();
                }
            });

            return true;
        }

        private async Task RunDeployment()
        {
            var logger = new Logger();

            try
            {
                await DeploymentRunner.RunAsync(options, logger);

                logger.Log(log);
            }
            catch (Exception ex)
            {
                lifetime.StopApplication();

                logger.Log(log, ex);
            }
        }

        private void StartConsumers()
        {
            foreach (var consumer in kafkaConsumers)
            {
                consumer.Start();
            }
        }
    }
}
