// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Core.EventSynchronization
{
    public static class SchemaSynchronizer
    {
        public static IEnumerable<IEvent> Synchronize(this Schema source, Schema? target, Func<long> idGenerator,
            SchemaSynchronizationOptions? options = null)
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(idGenerator, nameof(idGenerator));

            if (target == null)
            {
                yield return new SchemaDeleted();
            }
            else
            {
                options ??= new SchemaSynchronizationOptions();

                static SchemaEvent E(SchemaEvent @event)
                {
                    return @event;
                }

                if (!source.Properties.Equals(target.Properties))
                {
                    yield return E(new SchemaUpdated { Properties = target.Properties });
                }

                if (!source.Category.StringEquals(target.Category))
                {
                    yield return E(new SchemaCategoryChanged { Name = target.Category });
                }

                if (!source.Scripts.Equals(target.Scripts))
                {
                    yield return E(new SchemaScriptsConfigured { Scripts = target.Scripts });
                }

                if (!source.PreviewUrls.EqualsDictionary(target.PreviewUrls))
                {
                    yield return E(new SchemaPreviewUrlsConfigured { PreviewUrls = target.PreviewUrls.ToDictionary() });
                }

                if (source.IsPublished != target.IsPublished)
                {
                    yield return target.IsPublished ?
                        E(new SchemaPublished()) :
                        E(new SchemaUnpublished());
                }

                var events = SyncFields(source.FieldCollection, target.FieldCollection, idGenerator, CanUpdateRoot, null, options);

                foreach (var @event in events)
                {
                    yield return E(@event);
                }

                if (!source.FieldsInLists.SequenceEqual(target.FieldsInLists))
                {
                    yield return E(new SchemaUIFieldsConfigured { FieldsInLists = target.FieldsInLists });
                }

                if (!source.FieldsInReferences.SequenceEqual(target.FieldsInReferences))
                {
                    yield return E(new SchemaUIFieldsConfigured { FieldsInReferences = target.FieldsInReferences });
                }

                if (!source.FieldRules.SetEquals(target.FieldRules))
                {
                    yield return E(new SchemaFieldRulesConfigured { FieldRules = target.FieldRules });
                }
            }
        }

        private static IEnumerable<SchemaEvent> SyncFields<T>(
            FieldCollection<T> source,
            FieldCollection<T> target,
            Func<long> idGenerator,
            Func<T, T, bool> canUpdate,
            NamedId<long>? parentId, SchemaSynchronizationOptions options) where T : class, IField
        {
            FieldEvent E(FieldEvent @event)
            {
                @event.ParentFieldId = parentId;

                return @event;
            }

            var sourceIds = source.Ordered.Select(x => x.NamedId()).ToList();

            if (!options.NoFieldDeletion)
            {
                foreach (var sourceField in source.Ordered)
                {
                    if (!target.ByName.TryGetValue(sourceField.Name, out _))
                    {
                        var id = sourceField.NamedId();

                        sourceIds.Remove(id);

                        yield return E(new FieldDeleted { FieldId = id });
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
                            yield return E(new FieldUpdated { FieldId = id, Properties = targetField.RawProperties });
                        }
                    }
                    else if (!sourceField.IsLocked && !options.NoFieldRecreation)
                    {
                        canCreateField = true;

                        sourceIds.Remove(id);

                        yield return E(new FieldDeleted { FieldId = id });
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
                        ParentFieldId = parentId,
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
                        yield return E(new FieldLocked { FieldId = id });
                    }

                    if (!targetField.IsHidden.BoolEquals(sourceField?.IsHidden))
                    {
                        yield return targetField.IsHidden ?
                            E(new FieldHidden { FieldId = id }) :
                            E(new FieldShown { FieldId = id });
                    }

                    if (!targetField.IsDisabled.BoolEquals(sourceField?.IsDisabled))
                    {
                        yield return targetField.IsDisabled ?
                            E(new FieldDisabled { FieldId = id }) :
                            E(new FieldEnabled { FieldId = id });
                    }

                    if ((sourceField == null || sourceField is IArrayField) && targetField is IArrayField targetArrayField)
                    {
                        var fields = (sourceField as IArrayField)?.FieldCollection ?? FieldCollection<NestedField>.Empty;

                        var events = SyncFields(fields, targetArrayField.FieldCollection, idGenerator, CanUpdate, id, options);

                        foreach (var @event in events)
                        {
                            yield return @event;
                        }
                    }
                }
            }

            if (sourceIds.Count > 1)
            {
                var sourceNames = sourceIds.Select(x => x.Name).ToList();
                var targetNames = target.Ordered.Select(x => x.Name).ToList();

                if (sourceNames.SetEquals(targetNames) && !sourceNames.SequenceEqual(targetNames))
                {
                    var fieldIds = targetNames.Select(x => sourceIds.Find(y => y.Name == x)!.Id).ToArray();

                    yield return new SchemaFieldsReordered { FieldIds = fieldIds, ParentFieldId = parentId };
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
}
