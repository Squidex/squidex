// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas.Commands;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    public sealed class ConfigureUIFieldsDto
    {
        /// <summary>
        /// The name of fields that are used in content lists.
        /// </summary>
        public List<string>? FieldsInLists { get; set; }

        /// <summary>
        /// The name of fields that are used in content references.
        /// </summary>
        public List<string>? FieldsInReferences { get; set; }

        public ConfigureUIFields ToCommand()
        {
            var command = new ConfigureUIFields();

            if (FieldsInLists != null)
            {
                command.FieldsInLists = new FieldNames(FieldsInLists);
            }

            if (FieldsInReferences != null)
            {
                command.FieldsInReferences = new FieldNames(FieldsInReferences);
            }

            return command;
        }
    }
}
