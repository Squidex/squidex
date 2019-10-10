using System.Collections.Generic;

namespace DeploymentApp.Entities
{
    public static class TestData
    {
        public static (string Id, object Data)[] CommentaryTypes =
        {
            ("1", new Dictionary<string, object>
            {
                ["id"] = new
                {
                    iv = "1"
                },
                ["name"] = new
                {
                    iv = "Overview"
                },
                ["character-limit"] = new
                {
                    iv = 800
                },
                ["requires-period"] = new
                {
                    iv = false
                }
            }),
            ("2", new Dictionary<string, object>
            {
                ["id"] = new
                {
                    iv = "2"
                },
                ["name"] = new
                {
                    iv = "Price Commentary"
                },
                ["character-limit"] = new
                {
                },
                ["requires-period"] = new
                {
                    iv = true
                }
            }),
            ("3", new Dictionary<string, object>
            {
                ["id"] = new
                {
                    iv = "3"
                },
                ["name"] = new
                {
                    iv = "Deals Commentary"
                },
                ["character-limit"] = new
                {
                    iv = 1800
                },
                ["requires-period"] = new
                {
                    iv = false
                }
            }),
            ("4", new Dictionary<string, object>
            {
                ["id"] = new
                {
                    iv = "4"
                },
                ["name"] = new
                {
                    iv = "Charts Commentary"
                },
                ["character-limit"] = new
                {
                    iv = 1800
                },
                ["requires-period"] = new
                {
                    iv = true
                }
            }),
            ("5", new Dictionary<string, object>
            {
                ["id"] = new
                {
                    iv = "5"
                },
                ["name"] = new
                {
                    iv = "Analyst Commentary"
                },
                ["character-limit"] = new
                {
                    iv = 1800
                },
                ["requires-period"] = new
                {
                    iv = false
                }
            }),
            ("6", new Dictionary<string, object>
            {
                ["id"] = new
                {
                    iv = "6"
                },
                ["name"] = new
                {
                    iv = "Outlook"
                },
                ["character-limit"] = new
                {
                    iv = 800
                },
                ["requires-period"] = new
                {
                    iv = false
                }
            })
        };

        public static (string Id, string Name)[] Periods =
        {
            ("http://iddn.icis.com/ref-data/period/9", "Settlement"),
            ("http://iddn.icis.com/ref-data/period/17", "Half Year"),
            ("http://iddn.icis.com/ref-data/period/8", "n/a"),
            ("http://iddn.icis.com/ref-data/period/20", "Variable"),
            ("http://iddn.icis.com/ref-data/period/13", "Day"),
            ("http://iddn.icis.com/ref-data/period/14", "Week"),
            ("http://iddn.icis.com/ref-data/period/15", "Month")
        };

        public static (string Id, string Name)[] Regions =
        {
            ("http://iddn.icis.com/ref-data/location/1", "Africa"),
            ("http://iddn.icis.com/ref-data/location/2", "CIS/Central Asia"),
            ("http://iddn.icis.com/ref-data/location/3", "Latin America"),
            ("http://iddn.icis.com/ref-data/location/4", "Middle East"),
            ("http://iddn.icis.com/ref-data/location/5", "North America"),
            ("http://iddn.icis.com/ref-data/location/6", "North East Asia"),
            ("http://iddn.icis.com/ref-data/location/7", "South East Asia & Pacific"),
            ("http://iddn.icis.com/ref-data/location/8", "South Asia"),
            ("http://iddn.icis.com/ref-data/location/9", "Europe")
        };

        public static (string Id, string Name)[] Commodities =
        {
            ("http://iddn.icis.com/ref-data/commodity-group/306", "Benzene"),
            ("http://iddn.icis.com/ref-data/commodity-group/297", "Styrene"),
            ("http://iddn.icis.com/ref-data/commodity-group/299", "Toluene"),
            ("http://iddn.icis.com/ref-data/commodity-group/300", "Acetone"),
            ("http://iddn.icis.com/ref-data/commodity-group/296", "Ployethylene"),
            ("http://iddn.icis.com/ref-data/commodity-group/290", "Polyols"),
            ("http://iddn.icis.com/ref-data/commodity-group/285", "Propylene"),
            ("http://iddn.icis.com/ref-data/commodity-group/295", "Ethylene Glycol")
        };

        public static (string CreateFor, string CommentaryType, string Commodity, string Region, string Body)[] Commentaries =
        {
            (
                "2019-09-14T00:00:00Z",
                "1",
                "http://iddn.icis.com/ref-data/commodity-group/306",
                "http://iddn.icis.com/ref-data/location/1",
                "Body1"
            )
        };
    }
}
