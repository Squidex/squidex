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
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Core.EventSynchronization
{
    public static class SchemaSynchronizer
    {
        public static IEnumerable<IEvent> Synchronize(this Schema source, Schema target, IJsonSerializer serializer, Func<long> idGenerator, SchemaSynchronizationOptions options = null)
        {
            Guard.NotNull(source, nameof(source));
            Guard.NotNull(serializer, nameof(serializer));
            Guard.NotNull(idGenerator, nameof(idGenerator));

            if (target == null)
            {
                yield return new SchemaDeleted();
            }
            else
            {
                options = options ?? new SchemaSynchronizationOptions();

                SchemaEvent E(SchemaEvent @event)
                {
                    return @event;
                }

                if (!source.Properties.EqualsJson(target.Properties, serializer))
                {
                    yield return E(new SchemaUpdated { Properties = target.Properties });
                }

                if (!source.Category.StringEquals(target.Category))
                {
                    yield return E(new SchemaCategoryChanged { Name = target.Category });
                }

                if (!source.Scripts.EqualsJson(target.Scripts, serializer))
                {
                    yield return E(new SchemaScriptsConfigured { Scripts = target.Scripts });
                }

                if (!source.PreviewUrls.EqualsDictionary(target.PreviewUrls))
                {
                    yield return E(new SchemaPreviewUrlsConfigured { PreviewUrls = target.PreviewUrls.ToDictionary(x => x.Key, x => x.Value) });
                }

                if (source.IsPublished != target.IsPublished)
                {
                    yield return target.IsPublished ?
                        E(new SchemaPublished()) :
                        E(new SchemaUnpublished());
                }

                var events = SyncFields(source.FieldCollection, target.FieldCollection, serializer, idGenerator, null, options);

                foreach (var @event in events)
                {
                    yield return E(@event);
                }
            }
        }

        private static IEnumerable<SchemaEvent> SyncFields<T>(
            FieldCollection<T> source,
            FieldCollection<T> target,
            IJsonSerializer serializer,
            Func<long> idGenerator,
            NamedId<long> parentId, SchemaSynchronizationOptions options) where T : class, IField
        {
            FieldEvent E(FieldEvent @event)
            {
                @event.ParentFieldId = parentId;

                return @event;
            }

            var sourceIds = new List<NamedId<long>>(source.Ordered.Select(x => x.NamedId()));
            var sourceNames = sourceIds.Select(x => x.Name).ToList();

            if (!options.NoFieldDeletion)
            {
                foreach (var sourceField in source.Ordered)
                {
                    if (!target.ByName.TryGetValue(sourceField.Name, out _))
                    {
                        var id = sourceField.NamedId();

                        sourceIds.Remove(id);
                        sourceNames.Remove(id.Name);

                        yield return E(new FieldDeleted { FieldId = id });
                    }
                }
            }

            foreach (var targetField in target.Ordered)
            {
                NamedId<long> id = null;

                var canCreateField = true;

                if (source.ByName.TryGetValue(targetField.Name, out var sourceField))
                {
                    canCreateField = false;

                    id = sourceField.NamedId();

                    if (CanUpdate(sourceField, targetField))
                    {
                        if (!sourceField.RawProperties.EqualsJson(targetField.RawProperties, serializer))
                        {
                            yield return E(new FieldUpdated { FieldId = id, Properties = targetField.RawProperties });
                        }
                    }
                    else if (!sourceField.IsLocked && !options.NoFieldRecreation)
                    {
                        canCreateField = true;

                        sourceIds.Remove(id);
                        sourceNames.Remove(id.Name);

                        yield return E(new FieldDeleted { FieldId = id });
                    }
                }

                if (canCreateField)
                {
                    var partitioning = (string)null;

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
                    sourceNames.Add(id.Name);
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
                        var fields = ((IArrayField)sourceField)?.FieldCollection ?? FieldCollection<NestedField>.Empty;

                        var events = SyncFields(fields, targetArrayField.FieldCollection, serializer, idGenerator, id, options);

                        foreach (var @event in events)
                        {
                            yield return @event;
                        }
                    }
                }
            }

            if (sourceNames.Count > 1)
            {
                var targetNames = target.Ordered.Select(x => x.Name);

                if (sourceNames.Intersect(targetNames).Count() == target.Ordered.Count && !sourceNames.SequenceEqual(targetNames))
                {
                    yield return new SchemaFieldsReordered { FieldIds = sourceIds.Select(x => x.Id).ToList(), ParentFieldId = parentId };
                }
            }
        }

        private static bool CanUpdate(IField source, IField target)
        {
            return !source.IsLocked && source.Name == target.Name && source.RawProperties.TypeEquals(target.RawProperties);
        }
    }
}
