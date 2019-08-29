using DeploymentApp.Extensions;

namespace DeploymentApp.Entities
{
    public static class Contributors
    {
        public static readonly ContributorFactory[] All =
        {
            () => ("vegatesteditor@cha.rbxd.ds", "CMS Editor"),
            () => ("vegatestreviewer@cha.rbxd.ds", "CMS Copy Editor"),
            () => ("vegatestadmin@cha.rbxd.ds", "Owner")
        };
    }
}
