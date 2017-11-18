// ==========================================================================
//  EntityCreatedDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Controllers.Api
{
    public sealed class EntityCreatedDto
    {
        /// <summary>
        /// Id of the created entity.
        /// </summary>
        [Required]
        public string Id { get; set; }

        /// <summary>
        /// The new version of the entity.
        /// </summary>
        public long Version { get; set; }
    }
}
