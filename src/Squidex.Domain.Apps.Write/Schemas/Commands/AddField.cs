// ==========================================================================
//  AddField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Write.Schemas.Guards;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Schemas.Commands
{
    public sealed class AddField : SchemaAggregateCommand, IValidatable
    {
        public string Name { get; set; }

        public string Partitioning { get; set; }

        public FieldProperties Properties { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (!Partitioning.IsValidPartitioning())
            {
                errors.Add(new ValidationError("Partitioning is not valid.", nameof(Partitioning)));
            }

            if (!Name.IsPropertyName())
            {
                errors.Add(new ValidationError("Name must be a valid property name.", nameof(Name)));
            }

            if (Properties == null)
            {
                errors.Add(new ValidationError("Properties must be defined.", nameof(Properties)));
            }
            else
            {
                var propertyErrors = FieldPropertiesValidator.Validate(Properties);

                foreach (var error in propertyErrors)
                {
                    errors.Add(error);
                }
            }
        }
    }
}