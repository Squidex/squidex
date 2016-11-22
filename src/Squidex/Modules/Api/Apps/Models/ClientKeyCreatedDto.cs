// ==========================================================================
//  ClientKeyCreatedDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Modules.Api.Apps.Models
{
    public sealed class ClientKeyCreatedDto
    {
        /// <summary>
        /// The created client key.
        /// </summary>
        [Required]
        public string ClientKey { get; set; }
    }
}
