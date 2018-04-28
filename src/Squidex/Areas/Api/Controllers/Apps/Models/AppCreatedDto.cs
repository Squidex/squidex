// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
        [JsonConverter(typeof(StringEnumConverter))]
        public AppContributorPermission Permission { get; set; }

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

        public static AppCreatedDto FromResult(EntityCreatedResult<Guid> result, IAppPlansProvider apps)
        {
            var response = new AppCreatedDto { Id = result.IdOrValue.ToString(), Version = result.Version };

            response.Permission = AppContributorPermission.Owner;

            response.PlanName = apps.GetPlan(null)?.Name;
            response.PlanUpgrade = apps.GetPlanUpgrade(null)?.Name;

            return response;
        }
    }
}
