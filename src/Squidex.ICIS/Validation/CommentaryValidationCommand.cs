using System;
using System.Linq;
using System.Threading.Tasks;
using NodaTime;
using NodaTime.Text;
using Orleans;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.ICIS.Validation
{
    public sealed class CommentaryValidationCommand : ICustomCommandMiddleware
    {
        private readonly IContentQueryService contentQuery;
        private readonly IContextProvider contextProvider;
        private readonly IGrainFactory grainFactory;

        public CommentaryValidationCommand(
            IContentQueryService contentQuery,
            IContextProvider contextProvider, 
            IGrainFactory grainFactory)
        {
            this.contentQuery = contentQuery;
            this.contextProvider = contextProvider;
            this.grainFactory = grainFactory;
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (context.Command is CreateContent createContent)
            {
                await ValidateContentAsync(createContent.ContentId, createContent.Data, createContent.SchemaId);
            }
            else if (context.Command is UpdateContent updateContent)
            {
                var content = await grainFactory.GetGrain<IContentGrain>(updateContent.ContentId).GetStateAsync();

                await ValidateContentAsync(updateContent.ContentId, updateContent.Data, content.Value.SchemaId);
            }

            await next();
        }

        private async Task ValidateContentAsync(Guid contentId, NamedContentData data, NamedId<Guid> schemaId)
        {
            var (isFound, regionId, commodityId, contentType, createdFor) = GetValues(data);

            if (isFound)
            {
                var query = CreateQuery(regionId, commodityId, contentType, createdFor);

                var contents = await contentQuery.QueryAsync(contextProvider.Context, schemaId.Name, Q.Empty.WithODataQuery(query));

                if (contents.Any(x => x.Id != contentId))
                {
                    throw new DomainException("A content item with these values already exists.");
                }
            }
        }

        private string CreateQuery(Guid regionId, Guid commodityId, Guid contentType, Instant createdFor)
        {
            return $"$top=2&$filter=data/region/iv eq '{regionId}' and data/commodity/iv eq '{commodityId}' and data/commentarytype/iv eq '{contentType}' and data/createdfor/iv eq {createdFor}";
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
