using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.ClientLibrary.Management;
using DeploymentApp.Utilities;
using Squidex.ClientLibrary;

namespace DeploymentApp.Extensions
{
    public delegate (string SchemaName, string TopicName) KafkaRuleFactory();

    public static class IcisClientWorkflowExtensions
    {
        public static async Task UpsertKafkaRule(this SquidexClientManager clientManager, KafkaRuleFactory factory)
        {
            var (schemaName, topicName) = factory();

            var rulesClient = new RulesClient(clientManager.CreateHttpClient());
            var rulesList = await rulesClient.GetRulesAsync(clientManager.App);

            var schemasClient = clientManager.CreateSchemasClient();
            var schemaId = schemasClient.GetSchemaAsync(clientManager.App, schemaName).Result.Id;

            var existingRule = rulesList.Items.FirstOrDefault(x =>
            {
                return x.Trigger is ContentChangedRuleTriggerDto t && t.Schemas?.Any(y => y.SchemaId.Equals(schemaId)) == true;
            });

            var trigger = new ContentChangedRuleTriggerDto
            {
                Schemas = new List<ContentChangedRuleTriggerSchemaDto>()
            };

            trigger.Schemas.Add(
                new ContentChangedRuleTriggerSchemaDto
                {
                    SchemaId = schemaId,
                    Condition = "event.type == 'Published' || (event.type == 'Updated' && event.status == 'Published')"
                }
            );

            try
            {
                if (existingRule != null)
                {
                    ConsoleHelper.Start($"Updating rule for schema {schemaName}");

                    var command = new UpdateRuleDto()
                    {
                        Action = new ICISKafkaRuleActionDto()
                        {
                            TopicName = topicName
                        },
                        Trigger = trigger
                    };

                    await rulesClient.PutRuleAsync(clientManager.App, existingRule.Id.ToString(), command);

                    ConsoleHelper.Success();
                }
                else
                {
                    ConsoleHelper.Start($"Creating rule");

                    var command = new CreateRuleDto
                    {
                        Action = new ICISKafkaRuleActionDto()
                        {
                            TopicName = topicName
                        },
                        Trigger = trigger
                    };

                    await rulesClient.PostRuleAsync(clientManager.App, command);

                    ConsoleHelper.Success();
                }

            }
            catch (Exception e)
            {
                ConsoleHelper.Failed(e);
                throw;
            }
        }
    }
}
