// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using System.Net.Http;

namespace Squidex.Web
{
    public class ResourceLink
    {
        [Required]
        [Display(Description = "The link url.")]
        public string Href { get; set; }

        [Required]
        [Display(Description = "The link method.")]
        public HttpMethod Method { get; set; } 
    }
}
