// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.EventSynchronization;

public static class SchemaSynchronizer
{
    public static IEnumerable<SchemaEvent> Synchronize(this Schema source, Schema? target, Func<long> idGenerator,
        SchemaSynchronizationOptions? options = null)
    {
        Guard.NotNull(source);
        Guard.NotNull(idGenerator);

        if (target == null)
        {
            yield return new SchemaDeleted();
        }
        else
        {
            options ??= new SchemaSynchronizationOptions();

            if (!source.Properties.Equals(target.Properties))
            {
                yield return new SchemaUpdated { Properties = target.Properties };
            }

            if (!source.Category.StringEquals(target.Category))
            {
                yield return new SchemaCategoryChanged { Name = target.Category };
            }

            if (!source.Scripts.Equals(target.Scripts))
            {
                yield return new SchemaScriptsConfigured { Scripts = target.Scripts };
            }

            if (!source.PreviewUrls.EqualsDictionary(target.PreviewUrls))
            {
                yield return new SchemaPreviewUrlsConfigured { PreviewUrls = target.PreviewUrls };
            }

            if (source.IsPublished != target.IsPublished)
            {
                yield return target.IsPublished ?
                    new SchemaPublished() :
                    new SchemaUnpublished();
            }

            var events = SyncFields(source.FieldCollection, target.FieldCollection, idGenerator, CanUpdateRoot, options);

            foreach (var @event in events)
            {
                yield return @event;
            }

            if (!source.FieldsInLists.Equals(target.FieldsInLists))
            {
                yield return new SchemaUIFieldsConfigured { FieldsInLists = target.FieldsInLists };
            }

            if (!source.FieldsInReferences.Equals(target.FieldsInReferences))
            {
                yield return new SchemaUIFieldsConfigured { FieldsInReferences = target.FieldsInReferences };
            }

            if (!source.FieldRules.Equals(target.FieldRules))
            {
                yield return new SchemaFieldRulesConfigured { FieldRules = target.FieldRules };
            }
        }
    }

    private static IEnumerable<ParentFieldEvent> SyncFields<T>(
        FieldCollection<T> source,
        FieldCollection<T> target,
        Func<long> idGenerator,
        Func<T, T, bool> canUpdate,
        SchemaSynchronizationOptions options) where T : class, IField
    {
        var sourceIds = source.Ordered.Select(x => x.NamedId()).ToList();

        if (!options.NoFieldDeletion)
        {
            foreach (var sourceField in source.Ordered)
            {
                if (!target.ByName.TryGetValue(sourceField.Name, out _))
                {
                    var id = sourceField.NamedId();

                    sourceIds.Remove(id);

                    yield return new FieldDeleted { FieldId = id };
                }
            }
        }

        foreach (var targetField in target.Ordered)
        {
            NamedId<long>? id = null;

            var canCreateField = true;

            if (source.ByName.TryGetValue(targetField.Name, out var sourceField))
            {
                canCreateField = false;

                id = sourceField.NamedId();

                if (canUpdate(sourceField, targetField))
                {
                    if (!sourceField.RawProperties.Equals(targetField.RawProperties as object))
                    {
                        yield return new FieldUpdated { FieldId = id, Properties = targetField.RawProperties };
                    }
                }
                else if (!sourceField.IsLocked && !options.NoFieldRecreation)
                {
                    canCreateField = true;

                    sourceIds.Remove(id);

                    yield return new FieldDeleted { FieldId = id };
                }
            }

            if (canCreateField)
            {
                var partitioning = (string?)null;

                if (targetField is IRootField rootField)
                {
                    partitioning = rootField.Partitioning.Key;
                }

                id = NamedId.Of(idGenerator(), targetField.Name);

                yield return new FieldAdded
                {
                    Name = targetField.Name,
                    Partitioning = partitioning,
                    Properties = targetField.RawProperties,
                    FieldId = id
                };

                sourceIds.Add(id);
            }

            if (id != null && (sourceField == null || CanUpdate(sourceField, targetField)))
            {
                if (!targetField.IsLocked.BoolEquals(sourceField?.IsLocked))
                {
                    yield return new FieldLocked { FieldId = id };
                }

                if (!targetField.IsHidden.BoolEquals(sourceField?.IsHidden))
                {
                    yield return targetField.IsHidden ?
                        new FieldHidden { FieldId = id } :
                        new FieldShown { FieldId = id };
                }

                if (!targetField.IsDisabled.BoolEquals(sourceField?.IsDisabled))
                {
                    yield return targetField.IsDisabled ?
                        new FieldDisabled { FieldId = id } :
                        new FieldEnabled { FieldId = id };
                }

                if (sourceField is null or IArrayField && targetField is IArrayField targetArrayField)
                {
                    var fields = (sourceField as IArrayField)?.FieldCollection ?? FieldCollection<NestedField>.Empty;

                    var events = SyncFields(fields, targetArrayField.FieldCollection, idGenerator, CanUpdate, options);

                    foreach (var @event in events)
                    {
                        @event.ParentFieldId = id;

                        yield return @event;
                    }
                }
            }
        }

        if (sourceIds.Count > 1)
        {
            var sourceNames = sourceIds.Select(x => x.Name).ToHashSet();
            var targetNames = target.Ordered.Select(x => x.Name).ToHashSet();

            if (sourceNames.SetEquals(targetNames) && !sourceNames.SequenceEqual(targetNames))
            {
                var fieldIds = targetNames.Select(x => sourceIds.Find(y => y.Name == x)!.Id).ToArray();

                yield return new SchemaFieldsReordered { FieldIds = fieldIds };
            }
        }
    }

    private static bool CanUpdateRoot(IRootField source, IRootField target)
    {
        return CanUpdate(source, target) && source.Partitioning == target.Partitioning;
    }

    private static bool CanUpdate(IField source, IField target)
    {
        return !source.IsLocked && source.Name == target.Name && source.RawProperties.TypeEquals(target.RawProperties);
    }
}
