// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Infrastructure.Validation;

namespace Squidex.Web;

public class ResourceLink
{
    [LocalizedRequired]
    [Display(Description = "The link url.")]
    public string Href { get; set; }

    [LocalizedRequired]
    [Display(Description = "The link method.")]
    public string Method { get; set; }

    [Display(Description = "Additional data about the link.")]
    public string? Metadata { get; set; }
}
