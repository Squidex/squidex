using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.ICIS.Utilities;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Validation;

namespace Squidex.ICIS.Validation
{
    public class CommentaryUniquenessValidator : ICommentaryValidator
    {
        private readonly IContentQueryService contentQuery;

        public CommentaryUniquenessValidator(IContentQueryService contentQuery)
        {
            this.contentQuery = contentQuery;
        }

        public async Task<IEnumerable<ValidationError>> ValidateCommentaryAsync(Guid contentId, NamedId<Guid> schemaId, Context context, NamedContentData data)
        {
            var (isFound, regionId, commodityId, contentTypeId, createdFor) = GetValues(data);

            if (isFound)
            {
                var query = CreateQuery(regionId, commodityId, contentTypeId, createdFor);

                var contents = await contentQuery.QueryAsync(context, schemaId.Name, Q.Empty.WithODataQuery(query));

                return ValidateCommentaryUniqueness(contentId, contents);
            }

            return null;
        }

        private IEnumerable<ValidationError> ValidateCommentaryUniqueness(Guid contentId, IResultList<IEnrichedContentEntity> contents)
        {
            if (contents.Any(x => x.Id != contentId))
            {
                yield return new ValidationError("A content item with these values already exists.");
            }
        }


        private string CreateQuery(Guid regionId, Guid commodityId, Guid contentTypeId, Instant createdFor)
        {
            return $"$top=2&$filter=data/region/iv eq '{regionId}' and data/commodity/iv eq '{commodityId}' and data/commentarytype/iv eq '{contentTypeId}' and data/createdfor/iv eq {createdFor}";
        }

        private static (bool isFound, Guid regionId, Guid commodityId, Guid contentTypeId, Instant createdFor) GetValues(NamedContentData data)
        {
            if (data != null &&
                data.TryGetGuid("region", out var regionId) &&
                data.TryGetGuid("commodity", out var commodityId) &&
                data.TryGetGuid("commentarytype", out var contentTypeId) &&
                data.TryGetDateTime("createdfor", out var createdFor))
            {
                return (true, regionId, commodityId, contentTypeId, createdFor);
            }

            return default;
        }
    }
}