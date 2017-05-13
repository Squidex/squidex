// ==========================================================================
//  AddField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Core.Schemas;
using Squidex.Infrastructure;

using PartitioningOption = Squidex.Core.Partitioning;

namespace Squidex.Write.Schemas.Commands
{
    public class AddField : FieldCommand, IValidatable
    {
        private static readonly HashSet<string> AllowedPartitions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            PartitioningOption.Language.Key,
            PartitioningOption.Invariant.Key
        };

        public string Name { get; set; }

        public string Partitioning { get; set; }

        public FieldProperties Properties { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (Partitioning != null && !AllowedPartitions.Contains(Partitioning))
            {
                errors.Add(new ValidationError($"Partitioning must be one of {string.Join(", ", AllowedPartitions)}.", nameof(Partitioning)));
            }

            if (!Name.IsPropertyName())
            {
                errors.Add(new ValidationError("Name must be a valid property name", nameof(Name)));
            }

            if (Properties == null)
            {
                errors.Add(new ValidationError("Properties must be defined.", nameof(Properties)));
            }
        }
    }
}