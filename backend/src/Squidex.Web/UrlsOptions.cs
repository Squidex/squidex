// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure;

namespace Squidex.Web
{
    public sealed class UrlsOptions
    {
        private readonly HashSet<string> allTrustedHosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private string baseUrl;
        private string[] trustedHosts;

        public bool EnableXForwardedHost { get; set; }

        public bool EnforceHTTPS { get; set; }

        public string BaseUrl
        {
            get
            {
                return baseUrl;
            }
            set
            {
                if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
                {
                    allTrustedHosts.Add(uri.Host);
                }

                baseUrl = value;
            }
        }

        public string[] TrustedHosts
        {
            get
            {
                return trustedHosts;
            }
            set
            {
                foreach (var host in trustedHosts?.Where(x => !string.IsNullOrWhiteSpace(x)).OrEmpty()!)
                {
                    allTrustedHosts.Add(host);
                }

                trustedHosts = value;
            }
        }

        public bool IsAllowedHost(string? url)
        {
            return Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri) && IsAllowedHost(uri);
        }

        public bool IsAllowedHost(Uri uri)
        {
            return !uri.IsAbsoluteUri || allTrustedHosts.Contains(uri.Host);
        }

        public string BuildUrl(string path, bool trailingSlash = true)
        {
            if (string.IsNullOrWhiteSpace(BaseUrl))
            {
                throw new ConfigurationException("Configure BaseUrl with 'urls:baseUrl'.");
            }

            return BaseUrl.BuildFullUrl(path, trailingSlash);
        }
    }
}
