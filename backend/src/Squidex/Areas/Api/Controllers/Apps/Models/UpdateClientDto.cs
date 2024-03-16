// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class UpdateClientDto
    {
        /// <summary>
        /// The new display name of the client.
        /// </summary>
        [LocalizedStringLength(20)]
        public string? Name { get; set; }

        /// <summary>
        /// The role of the client.
        /// </summary>
        public string? Role { get; set; }

        /// <summary>
        /// True to allow anonymous access without an access token for this client.
        /// </summary>
        public bool? AllowAnonymous { get; set; }

        /// <summary>
        /// The number of allowed api calls per month for this client.
        /// </summary>
        public long? ApiCallsLimit { get; set; }

        /// <summary>
        /// The number of allowed api traffic bytes per month for this client.
        /// </summary>
        public long? ApiTrafficLimit { get; set; }

        public UpdateClient ToCommand(string clientId)
        {
            return SimpleMapper.Map(this, new UpdateClient { Id = clientId });
        }
    }
}
