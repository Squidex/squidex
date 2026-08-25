// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
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

    public static async Task<QueryModelDto> FromModelAsync(QueryModel model, App app, Schema? schema, IContentWorkflows workflows)
    {
        var result = SimpleMapper.Map(model, new QueryModelDto());

        if (schema != null)
        {
            using var workflow = await workflows.GetWorkflowAsync(app, schema);

            result.Statuses = workflow.GetAll().Select(StatusInfoDto.FromDomain).ToArray();
        }

        return result;
    }
}
