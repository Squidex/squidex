// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Areas.Api.Controllers.Contents.Models;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.Schemas.Models;

public sealed class QueryModelDto
{
    public FilterSchema Schema { get; init; } = FilterSchema.Any;

    public IReadOnlyDictionary<FilterSchemaType, IReadOnlyList<CompareOperator>> Operators { get; init; }

    public StatusInfoDto[] Statuses { get; set; }

    public static async Task<QueryModelDto> FromModelAsync(QueryModel model, Schema? schema, IContentWorkflow workflow)
    {
        var result = SimpleMapper.Map(model, new QueryModelDto());

        if (schema != null)
        {
            await result.AssignStatusesAsync(workflow, schema);
        }

        return result;
    }

    private async Task AssignStatusesAsync(IContentWorkflow workflow, Schema schema)
    {
        var allStatuses = await workflow.GetAllAsync(schema);

        Statuses = allStatuses.Select(StatusInfoDto.FromDomain).ToArray();
    }
}
