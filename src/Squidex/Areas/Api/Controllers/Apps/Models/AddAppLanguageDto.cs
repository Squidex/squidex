// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class AddAppLanguageDto
    {
        /// <summary>
        /// The language to add.
        /// </summary>
        [Required]
        public Language Language { get; set; }

        public AddLanguage ToCommand()
        {
            return SimpleMapper.Map(this, new AddLanguage());
        }
    }
}
