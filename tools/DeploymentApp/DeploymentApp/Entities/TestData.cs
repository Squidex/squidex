namespace DeploymentApp.Entities
{
    public static class TestData
    {
        public static (string Id, string Name)[] CommentaryTypes =
        {
            ("1", "Overview"),
            ("2", "Price Commentary"),
            ("3", "Deals Commentary"),
            ("4", "Charts Commentary"),
            ("5", "Analyst Commentary")
        };

        public static (string Id, string Name)[] Regions =
        {
            ("http://iddn.icis.com/ref-data/location/1", "Africa"),
            ("http://iddn.icis.com/ref-data/location/2", "CIS/Central Asia"),
            ("http://iddn.icis.com/ref-data/location/3", "Latin America"),
            ("http://iddn.icis.com/ref-data/location/5", "Middle East"),
            ("http://iddn.icis.com/ref-data/location/6", "North America"),
            ("http://iddn.icis.com/ref-data/location/7", "North East Asia"),
            ("http://iddn.icis.com/ref-data/location/8", "South East Asia & Pacific"),
            ("http://iddn.icis.com/ref-data/location/9", "South Asia")
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
