using System;
using System.Collections.Generic;

namespace DeploymentApp
{
    public sealed class DeploymentOptions
    {
        public string App { get; set; } = "commentary";

        public string Url { get; set; } = "http://localhost:5000";

        public string IdentityServer { get; set; } = "http://identityservice.systest.tesla.cha.rbxd.ds/connect/token";

        public string ClientId { get; set; } = "CMSDeployer";

        public string ClientSecret { get; set; } = "p@55w0rd";

        public bool SkipRules { get; set; }

        public bool GenerateTestData { get; set; }

        public bool Validate()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(App))
            {
                errors.Add("App: Not defined.");
            }

            if (string.IsNullOrWhiteSpace(Url))
            {
                errors.Add("Url: Not defined.");
            }
            else if (!Uri.IsWellFormedUriString(Url, UriKind.Absolute))
            {
                errors.Add("Url: Not a valid url.");
            }

            if (string.IsNullOrWhiteSpace(IdentityServer))
            {
                errors.Add("IdentityServer: Not defined.");
            }
            else if (!Uri.IsWellFormedUriString(IdentityServer, UriKind.Absolute))
            {
                errors.Add("IdentityServer: Not a valid url.");
            }

            if (string.IsNullOrWhiteSpace(ClientId))
            {
                errors.Add("ClientId: Not defined.");
            }

            if (string.IsNullOrWhiteSpace(ClientSecret))
            {
                errors.Add("ClientSecret: Not defined.");
            }

            if (errors.Count > 0)
            {
                Console.WriteLine("Configuration is not valid:");

                foreach (var error in errors)
                {
                    Console.WriteLine($" * {error}");
                }

                return false;
            }

            return true;
        }
    }
}
