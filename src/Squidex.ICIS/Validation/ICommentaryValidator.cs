using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.ICIS.Validation
{
    public interface ICommentaryValidator
    {
        Task<IEnumerable<ValidationError>> ValidateCommentaryAsync(Guid contentId, NamedId<Guid> schemaId, Context context, NamedContentData data);
    }
}