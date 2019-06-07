// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;

namespace Squidex.Web
{
    public abstract class Resource
    {
        [JsonProperty("_links")]
        [Required]
        [Display(Description = "The links.")]
        public Dictionary<string, ResourceLink> Links { get; } = new Dictionary<string, ResourceLink>();

        public void AddSelfLink(string href)
        {
            AddGetLink("self", href);
        }

        public void AddGetLink(string rel, string href)
        {
            AddLink(rel, "GET", href);
        }

        public void AddPatchLink(string rel, string href)
        {
            AddLink(rel, "PATCH", href);
        }

        public void AddPostLink(string rel, string href)
        {
            AddLink(rel, "POST", href);
        }

        public void AddPutLink(string rel, string href)
        {
            AddLink(rel, "PUT", href);
        }

        public void AddDeleteLink(string rel, string href)
        {
            AddLink(rel, "DELETE", href);
        }

        public void AddLink(string rel, string method, string href)
        {
            Links[rel] = new ResourceLink { Href = href, Method = method };
        }
    }
}
