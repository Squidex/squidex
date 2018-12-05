// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Infrastructure.Commands;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class AppCreatedDto
    {
        /// <summary>
        /// Id of the created entity.
        /// </summary>
        [Required]
        public string Id { get; set; }

        /// <summary>
        /// The permission level of the user.
        /// </summary>
        public string[] Permissions { get; set; }

        /// <summary>
        /// The new version of the entity.
        /// </summary>
        public long Version { get; set; }

        /// <summary>
        /// Gets the current plan name.
        /// </summary>
        public string PlanName { get; set; }

        /// <summary>
        /// Gets the next plan name.
        /// </summary>
        public string PlanUpgrade { get; set; }

        public static AppCreatedDto FromResult(string name, EntityCreatedResult<Guid> result, IAppPlansProvider apps)
        {
            var response = new AppCreatedDto
            {
                Id = result.IdOrValue.ToString(),
                Permissions = Role.CreateOwner(name).Permissions.ToIds().ToArray(),
                PlanName = apps.GetPlan(null)?.Name,
                PlanUpgrade = apps.GetPlanUpgrade(null)?.Name,
                Version = result.Version
            };

            return response;
        }
    }
}
