// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Validation;

namespace Squidex.Areas.Api.Controllers.Apps.Models
{
    public sealed class AddLanguageDto
    {
        /// <summary>
        /// The language to add.
        /// </summary>
        [LocalizedRequired]
        public Language Language { get; set; }

        public AddLanguage ToCommand()
        {
            return SimpleMapper.Map(this, new AddLanguage());
        }
    }
}
