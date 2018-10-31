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
    public sealed class UpdateClientDto
    {
        /// <summary>
        /// The new display name of the client.
        /// </summary>
        [StringLength(20)]
        public string Name { get; set; }

        /// <summary>
        /// The role of the client.
        /// </summary>
        public string Role { get; set; }

        public UpdateClient ToCommand(string clientId)
        {
            return SimpleMapper.Map(this, new UpdateClient { Id = clientId });
        }
    }
}
