using System;
using System.Net.Http;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Squidex.ClientLibrary;

namespace DeploymentApp.Extensions
{
    public class IcisAuthenticatorExtensions : IAuthenticator
    {
        private readonly HttpClient httpClient = new HttpClient();
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly Uri serviceUrl;

        public IcisAuthenticatorExtensions(string serviceUrl, string clientId, string clientSecret)
            : this(new Uri(serviceUrl, UriKind.Absolute), clientId, clientSecret)
        {
        }

        public IcisAuthenticatorExtensions(Uri serviceUrl, string clientId, string clientSecret)
        {
            this.serviceUrl = serviceUrl;
            this.clientId = clientId;
            this.clientSecret = clientSecret;
        }

        public async Task<string> GetBearerTokenAsync()
        {
            var url = $"{serviceUrl}";

            var bodyString = $"grant_type=client_credentials&scope=all";
            var bodyContent = new StringContent(bodyString, Encoding.UTF8, "application/x-www-form-urlencoded");

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = bodyContent
            };

            AddBasicAuth(request);

            using (var response = await httpClient.SendAsync(request))
            {
                if (!response.IsSuccessStatusCode)
                {
                    throw new SecurityException($"Failed to retrieve access token for client '{clientId}', got HTTP {response.StatusCode}.");
                }

                var jsonString = await response.Content.ReadAsStringAsync();
                var jsonToken = JToken.Parse(jsonString);

                return jsonToken["access_token"].ToString();
            }
        }

        private void AddBasicAuth(HttpRequestMessage request)
        {
            var authInfo = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}"));

            request.Headers.Add("Authorization", $"Basic {authInfo}");
        }

        public Task RemoveTokenAsync(string token)
        {
            return Task.CompletedTask;
        }
    }
}