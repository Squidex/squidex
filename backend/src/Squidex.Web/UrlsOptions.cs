// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Squidex.Infrastructure;

namespace Squidex.Web
{
    public sealed class UrlsOptions
    {
        private readonly HashSet<HostString> allTrustedHosts = new HashSet<HostString>();
        private string baseUrl;
        private string[]? trustedHosts;

        public string[]? KnownProxies { get; set; }

        public bool EnableForwardHeaders { get; set; } = true;

        public bool EnforceHTTPS { get; set; } = false;

        public bool EnforceHost { get; set; } = false;

        public int? HttpsPort { get; set; } = 443;

        public string BaseUrl
        {
            get
            {
                return baseUrl;
            }
            set
            {
                if (TryBuildHost(value, out var host))
                {
                    allTrustedHosts.Add(host);
                }

                baseUrl = value;
            }
        }

        public string[]? TrustedHosts
        {
            get
            {
                return trustedHosts;
            }
            set
            {
                if (trustedHosts != null)
                {
                    foreach (var canidate in trustedHosts)
                    {
                        if (TryBuildHost(canidate, out var host))
                        {
                            allTrustedHosts.Add(host);
                        }
                    }
                }

                trustedHosts = value;
            }
        }

        public bool IsAllowedHost(string? url)
        {
            if (!Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri))
            {
                return false;
            }

            return IsAllowedHost(uri);
        }

        public bool IsAllowedHost(Uri uri)
        {
            if (!uri.IsAbsoluteUri)
            {
                return true;
            }

            return allTrustedHosts.Contains(BuildHost(uri));
        }

        public string BuildUrl(string path, bool trailingSlash = true)
        {
            if (string.IsNullOrWhiteSpace(BaseUrl))
            {
                throw new ConfigurationException("Configure BaseUrl with 'urls:baseUrl'.");
            }

            return BaseUrl.BuildFullUrl(path, trailingSlash);
        }

        public HostString BuildHost()
        {
            if (string.IsNullOrWhiteSpace(BaseUrl))
            {
                throw new ConfigurationException("Configure BaseUrl with 'urls:baseUrl'.");
            }

            if (!TryBuildHost(BaseUrl, out var host))
            {
                throw new ConfigurationException("Configure BaseUrl with 'urls:baseUrl' host name.");
            }

            return host;
        }

        private static bool TryBuildHost(string urlOrHost, out HostString host)
        {
            host = default;

            if (string.IsNullOrWhiteSpace(urlOrHost))
            {
                return false;
            }

            if (Uri.TryCreate(urlOrHost, UriKind.Absolute, out var uri1))
            {
                host = BuildHost(uri1);

                return true;
            }

            if (Uri.TryCreate($"http://{urlOrHost}", UriKind.Absolute, out var uri2))
            {
                host = BuildHost(uri2);

                return true;
            }

            return false;
        }

        private static HostString BuildHost(Uri uri)
        {
            return BuildHost(uri.Host, uri.Port);
        }

        private static HostString BuildHost(string host, int port)
        {
            if (port == 443 || port == 80)
            {
                return new HostString(host.ToLowerInvariant());
            }
            else
            {
                return new HostString(host.ToLowerInvariant(), port);
            }
        }
    }
}
