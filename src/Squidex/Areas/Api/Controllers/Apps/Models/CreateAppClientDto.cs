// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class CreateAppClientDto
    {
        /// <summary>
        /// The id of the client.
        /// </summary>
        [Required]
        [RegularExpression("^[a-z0-9]+(\\-[a-z0-9]+)*$")]
        public string Id { get; set; }

        public AttachClient ToCommand()
        {
            return SimpleMapper.Map(this, new AttachClient());
        }
    }
}
