// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.Web;

public abstract class Resource
{
    [LocalizedRequired]
    [Display(Description = "The links.")]
    [JsonPropertyName("_links")]
    public Dictionary<string, ResourceLink> Links { get; } = new Dictionary<string, ResourceLink>();

    protected void AddSelfLink(string href)
    {
        AddGetLink("self", href);
    }

    protected void AddGetLink(string rel, string href, string? metadata = null)
    {
        AddLink(rel, "GET", href, metadata);
    }

    protected void AddPatchLink(string rel, string href, string? metadata = null)
    {
        AddLink(rel, "PATCH", href, metadata);
    }

    protected void AddPostLink(string rel, string href, string? metadata = null)
    {
        AddLink(rel, "POST", href, metadata);
    }

    protected void AddPutLink(string rel, string href, string? metadata = null)
    {
        AddLink(rel, "PUT", href, metadata);
    }

    protected void AddDeleteLink(string rel, string href, string? metadata = null)
    {
        AddLink(rel, "DELETE", href, metadata);
    }

    protected void AddLink(string rel, string method, string href, string? metadata = null)
    {
        Guard.NotNullOrEmpty(rel);
        Guard.NotNullOrEmpty(href);
        Guard.NotNullOrEmpty(method);

        Links[rel] = new ResourceLink { Href = href, Method = method, Metadata = metadata };
    }
}
