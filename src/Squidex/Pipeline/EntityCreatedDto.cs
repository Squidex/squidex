// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Infrastructure.Commands;

namespace Squidex.Pipeline
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

        public static EntityCreatedDto FromResult<T>(EntityCreatedResult<T> result)
        {
            return new EntityCreatedDto { Id = result.IdOrValue?.ToString(), Version = result.Version };
        }
    }
}
