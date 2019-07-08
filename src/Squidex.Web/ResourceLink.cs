// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Web
{
    public class ResourceLink
    {
        [Required]
        [Display(Description = "The link url.")]
        public string Href { get; set; }

        [Required]
        [Display(Description = "The link method.")]
        public string Method { get; set; }

        [Display(Description = "Additional data about the link.")]
        public string Metadata { get; set; }
    }
}
