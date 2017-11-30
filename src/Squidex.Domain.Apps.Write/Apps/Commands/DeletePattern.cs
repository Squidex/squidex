// ==========================================================================
//  DeletePattern.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Apps.Commands
{
    public sealed class DeletePattern : AppAggregateCommand, IValidatable
    {
        public string Name { get; set; }

        public void Validate(IList<ValidationError> errors)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                errors.Add(new ValidationError("Name is not defined", nameof(Name)));
            }
        }
    }
}
