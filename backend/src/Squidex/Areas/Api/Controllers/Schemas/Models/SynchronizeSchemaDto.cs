// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Schemas.Commands;

namespace Squidex.Areas.Api.Controllers.Schemas.Models
{
    public sealed class SynchronizeSchemaDto : UpsertSchemaDto
    {
        /// <summary>
        /// True, when fields should not be deleted.
        /// </summary>
        public bool NoFieldDeletion { get; set; }

        /// <summary>
        /// True, when fields with different types should not be recreated.
        /// </summary>
        public bool NoFieldRecreation { get; set; }

        public SynchronizeSchema ToCommand()
        {
            return ToCommand(this, new SynchronizeSchema());
        }
    }
}
