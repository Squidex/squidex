// ==========================================================================
//  UpdatePattern.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Apps.Commands
{
    public sealed class UpdatePattern : AppAggregateCommand, IValidatable
    {
        public string OriginalName { get; set; }

        public string OriginalPattern { get; set; }

        public string OriginalDefaultMessage { get; set; }

        public string Name { get; set; }

        public string Pattern { get; set; }

        public string DefaultMessage { get; set; }

        public Dictionary<Guid, Schema> Schemas { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (string.IsNullOrWhiteSpace(OriginalName))
            {
                errors.Add(new ValidationError("OriginalName is not defined", nameof(OriginalName)));
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                errors.Add(new ValidationError("Name is not defined", nameof(Name)));
            }

            if (string.IsNullOrWhiteSpace(Pattern))
            {
                errors.Add(new ValidationError("Pattern is not defined", nameof(Pattern)));
            }
        }
    }
}
