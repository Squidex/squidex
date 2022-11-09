// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Entities.Contents;

public sealed class DefaultWorkflowsValidator : IWorkflowsValidator
{
    private readonly IAppProvider appProvider;

    public DefaultWorkflowsValidator(IAppProvider appProvider)
    {
        this.appProvider = appProvider;
    }

    public async Task<IReadOnlyList<string>> ValidateAsync(DomainId appId, Workflows workflows)
    {
        Guard.NotNull(workflows);

        var errors = new List<string>();

        if (workflows.Values.Count(x => x.SchemaIds.Count == 0) > 1)
        {
            errors.Add(T.Get("workflows.overlap"));
        }

        var uniqueSchemaIds = workflows.Values.SelectMany(x => x.SchemaIds).Distinct().ToList();

        foreach (var schemaId in uniqueSchemaIds)
        {
            if (workflows.Values.Count(x => x.SchemaIds.Contains(schemaId)) > 1)
            {
                var schema = await appProvider.GetSchemaAsync(appId, schemaId);

                if (schema != null)
                {
                    errors.Add(T.Get("workflows.schemaOverlap", new { schema = schema.SchemaDef.Name }));
                }
            }
        }

        return errors;
    }
}
