// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Web
{
    public abstract class Resource
    {
        [LocalizedRequired]
        [Display(Description = "The links.")]
        [JsonProperty("_links")]
        public Dictionary<string, ResourceLink> Links { get; } = new Dictionary<string, ResourceLink>();

        public void AddSelfLink(string href)
        {
            AddGetLink("self", href);
        }

        public void AddGetLink(string rel, string href, string? metadata = null)
        {
            AddLink(rel, "GET", href, metadata);
        }

        public void AddPatchLink(string rel, string href, string? metadata = null)
        {
            AddLink(rel, "PATCH", href, metadata);
        }

        public void AddPostLink(string rel, string href, string? metadata = null)
        {
            AddLink(rel, "POST", href, metadata);
        }

        public void AddPutLink(string rel, string href, string? metadata = null)
        {
            AddLink(rel, "PUT", href, metadata);
        }

        public void AddDeleteLink(string rel, string href, string? metadata = null)
        {
            AddLink(rel, "DELETE", href, metadata);
        }

        public void AddLink(string rel, string method, string href, string? metadata = null)
        {
            Guard.NotNullOrEmpty(rel, nameof(rel));
            Guard.NotNullOrEmpty(href, nameof(href));
            Guard.NotNullOrEmpty(method, nameof(method));

            Links[rel] = new ResourceLink { Href = href, Method = method, Metadata = metadata };
        }
    }
}
