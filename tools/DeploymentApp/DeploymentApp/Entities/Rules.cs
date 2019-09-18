using DeploymentApp.Extensions;

namespace DeploymentApp.Entities
{
    public static class Rules
    {
        public static readonly KafkaRuleFactory[] AllKafkaRules =
        {
           () => ("commentary-type", "systest_cosmos_commentary_type_external_1"),
           () => ("commentary", "systest_cosmos_commentary_external_1")
        };
    }
}
