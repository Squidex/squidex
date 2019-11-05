// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class UpdateAppDto
    {
        /// <summary>
        /// The optional label of your app.
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// The optional description of your app.
        /// </summary>
        public string? Description { get; set; }

        public UpdateApp ToCommand()
        {
            return SimpleMapper.Map(this, new UpdateApp());
        }
    }
}
