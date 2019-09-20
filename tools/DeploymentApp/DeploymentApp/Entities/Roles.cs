using DeploymentApp.Extensions;

namespace DeploymentApp.Entities
{
    public static class Roles
    {
        public static readonly RoleFactory[] All =
        {
            Analyst,
            CopyEditor,
            Editor,
            ManagingAnalyst,
            ManagingEditor
        };

        public static (string Name, string[] Permissions) Analyst()
        {
            return ("CMS Analyst", new[]
            {
                "contents.commentary.read",
                "contents.commentary.create",
                "contents.commentary.archive",
                "contents.commentary.update",
                "contents.commentary-type.read",
                "contents.commodity.read",
                "contents.region.read"
            });
        }

        public static (string Name, string[] Permissions) CopyEditor()
        {
            return ("CMS Copy Editor", new[]
            {
                "contents.commentary.read",
                "contents.commentary.create",
                "contents.commentary.archive",
                "contents.commentary.update",
                "contents.commentary.publish",
                "contents.commentary-type.read",
                "contents.commodity.read",
                "contents.region.read"
            });
        }

        public static (string Name, string[] Permissions) Editor()
        {
            return ("CMS Editor", new[]
            {
                "contents.commentary.read",
                "contents.commentary.create",
                "contents.commentary.archive",
                "contents.commentary.update",
                "contents.commentary-type.read",
                "contents.commodity.read",
                "contents.region.read",
                "contents.period.read"
            });
        }

        public static (string Name, string[] Permissions) ManagingAnalyst()
        {
            return ("CMS Managing Analyst", new[]
            {
                "contents.commentary.read",
                "contents.commentary.create",
                "contents.commentary.archive",
                "contents.commentary.update",
                "contents.commentary.publish",
                "contents.commentary-type.read",
                "contents.commodity.read",
                "contents.region.read",
                "contents.period.read"
            });
        }

        public static (string Name, string[] Permissions) ManagingEditor()
        {
            return ("CMS Managing Editor", new[]
            {
                "contents.commentary.read",
                "contents.commentary.create",
                "contents.commentary.archive",
                "contents.commentary.update",
                "contents.commentary.publish",
                "contents.commentary-type.read",
                "contents.commodity.read",
                "contents.region.read",
                "contents.period.read"
            });
        }
    }
}
