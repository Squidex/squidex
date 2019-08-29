using DeploymentApp.Extensions;

namespace DeploymentApp.Entities
{
    public static class Rules
    {
        public static readonly KafkaRuleFactory[] AllKafkaRules =
        {
           () => ("commodity", "systest_cosmos_commodity_internal_2"),
           () => ("region", "systest_cosmos_region_internal_2"),
           () => ("commentary-type", "systest_cosmos_commentary-type_internal_2"),
           () => ("commentary", "systest_cosmos_commentary_internal_3")
        };
    }
}
