using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cosmos.ClientLibrary.Management;

namespace DeploymentApp.Extensions
{
    public delegate (string SchemaName, string TopicName) KafkaRuleFactory();

    public static class IcisClientWorkflowExtensions
    {
        public static async Task UpsertKafkaRule(this IcisClient client, KafkaRuleFactory factory)
        {
            var (schemaName, topicName) = factory();

            var rulesClient = new RulesClient(client.ClientManager.CreateHttpClient());
            var rulesList = await rulesClient.GetRulesAsync(client.ClientManager.App);

            var schemasClient = client.ClientManager.CreateSchemasClient();
            var schemaId = schemasClient.GetSchemaAsync(client.ClientManager.App, schemaName).Result.Id;

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
                    client.Log.Start($"Updating rule for schema {schemaName}");

                    var command = new UpdateRuleDto()
                    {
                        Action = new ICISKafkaRuleActionDto()
                        {
                            TopicName = topicName
                        },
                        Trigger = trigger
                    };

                    await rulesClient.PutRuleAsync(client.ClientManager.App, existingRule.Id.ToString(), command);

                    client.Log.Success();
                }
                else
                {
                    client.Log.Start($"Creating rule");

                    var command = new CreateRuleDto
                    {
                        Action = new ICISKafkaRuleActionDto()
                        {
                            TopicName = topicName
                        },
                        Trigger = trigger
                    };

                    await rulesClient.PostRuleAsync(client.ClientManager.App, command);

                    client.Log.Success();
                }

            }
            catch (Exception e)
            {
                client.Log.Failed(e);
                throw;
            }
        }
    }
}
