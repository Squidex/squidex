// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Infrastructure.Commands;

namespace Squidex.Web
{
    public sealed class EntityCreatedDto
    {
        [Required]
        [Display(Description = "Id of the created entity.")]
        public string Id { get; set; }

        [Display(Description = "The new version of the entity.")]
        public long Version { get; set; }

        public static EntityCreatedDto FromResult<T>(EntityCreatedResult<T> result)
        {
            return new EntityCreatedDto { Id = result.IdOrValue?.ToString(), Version = result.Version };
        }
    }
}
