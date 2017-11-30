/*
 * CivicPlus implementation of Squidex Headless CMS
 */

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Apps.Commands
{
    public sealed class AddPattern : AppAggregateCommand, IValidatable
    {
        public string Name { get; set; }

        public string Pattern { get; set; }

        public string DefaultMessage { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
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
