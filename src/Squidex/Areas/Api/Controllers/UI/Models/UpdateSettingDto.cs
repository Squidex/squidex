// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Newtonsoft.Json.Linq;

namespace Squidex.Areas.Api.Controllers.UI.Models
{
    public sealed class UpdateSettingDto
    {
        /// <summary>
        /// The value for the setting.
        /// </summary>
        public JToken Value { get; set; }
    }
}
