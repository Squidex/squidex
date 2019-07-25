using System;
using System.Threading.Tasks;
using NodaTime;
using NodaTime.Text;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.ICIS.Commands
{
    public sealed class UniqueContentValidationCommand : ICommandMiddleware
    {
        private readonly IContentQueryService contentQuery;
        private readonly IContextProvider contextProvider;

        public UniqueContentValidationCommand(IContentQueryService contentQuery, IContextProvider contextProvider)
        {
            this.contentQuery = contentQuery;
            this.contextProvider = contextProvider;
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (context.Command is CreateContent createContent)
            {
                var (isFound, regionId, commodityId, contentType, createdFor) = GetValues(createContent.Data);

                if (isFound)
                {
                    var query = CreateQuery(regionId, commodityId, contentType, createdFor);

                    var contents = await contentQuery.QueryAsync(contextProvider.Context, createContent.SchemaId.Name, Q.Empty.WithODataQuery(query));

                    if (contents.Total > 0)
                    {
                        throw new DomainException("A content item with these values already exists.");
                    }
                }
            }

            await next();
        }

        private string CreateQuery(Guid regionId, Guid commodityId, Guid contentType, Instant createdFor)
        {
            return $"$top=1&$filter=data/region/iv eq '{regionId}' and data/commodity/iv eq '{commodityId}' and data/commentarytype/iv eq '{contentType}' and data/createdfor/iv eq {createdFor}";
        }

        private static (bool isFound, Guid regionId, Guid commodityId, Guid contentType, Instant createdFor) GetValues(NamedContentData data)
        {
            if (data != null &&
                TryGetGuid(data, "region", out var regionId) &&
                TryGetGuid(data, "commodity", out var commodityId) &&
                TryGetGuid(data, "commentarytype", out var commentaryTypeId) &&
                TryGetDateTime(data, "createdfor", out var createdFor))
            {
                return (true, regionId, commodityId, commentaryTypeId, createdFor);
            }

            return default;
        }

        private static bool TryGetGuid(NamedContentData data, string field, out Guid id)
        {
            id = default;

            return data.TryGetValue(field, out var values) &&
                values.TryGetValue("iv", out var value) &&
                value != null &&
                value is JsonArray array &&
                array.Count == 1 &&
                array[0] != null &&
                Guid.TryParse(array[0].ToString(), out id);
        }

        private static bool TryGetDateTime(NamedContentData data, string field, out Instant dateTime)
        {
            dateTime = default;

            if (data.TryGetValue(field, out var values) &&
                values.TryGetValue("iv", out var value) &&
                value != null &&
                value is JsonString s)
            {
                var parsed = InstantPattern.General.Parse(s.ToString());

                if (parsed.Success)
                {
                    dateTime = parsed.Value;

                    return true;
                }
            }

            return false;
        }
    }
}
