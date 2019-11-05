﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core.EventSynchronization;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Xunit;

namespace Squidex.Domain.Apps.Core.Operations.EventSynchronization
{
    public class SchemaSynchronizerTests
    {
        private readonly Func<long> idGenerator;
        private readonly IJsonSerializer jsonSerializer = TestUtils.DefaultSerializer;
        private readonly NamedId<long> stringId = NamedId.Of(13L, "my-value");
        private readonly NamedId<long> nestedId = NamedId.Of(141L, "my-value");
        private readonly NamedId<long> arrayId = NamedId.Of(14L, "11-array");
        private int fields = 50;

        public SchemaSynchronizerTests()
        {
            idGenerator = () => fields++;
        }

        [Fact]
        public void Should_create_events_if_schema_deleted()
        {
            var sourceSchema =
                new Schema("source");

            var targetSchema =
                (Schema?)null;

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            events.ShouldHaveSameEvents(
                new SchemaDeleted()
            );
        }

        [Fact]
        public void Should_create_events_if_category_changed()
        {
            var sourceSchema =
                new Schema("source");

            var targetSchema =
                new Schema("target")
                    .ChangeCategory("Category");

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            events.ShouldHaveSameEvents(
                new SchemaCategoryChanged { Name = "Category" }
            );
        }

        [Fact]
        public void Should_create_events_if_scripts_configured()
        {
            var scripts = new SchemaScripts
            {
                Create = "<create-script>"
            };

            var sourceSchema =
                new Schema("source");

            var targetSchema =
                new Schema("target").ConfigureScripts(scripts);

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            events.ShouldHaveSameEvents(
                new SchemaScriptsConfigured { Scripts = scripts }
            );
        }

        [Fact]
        public void Should_create_events_if_preview_urls_configured()
        {
            var previewUrls = new Dictionary<string, string>
            {
                ["web"] = "Url"
            };

            var sourceSchema =
                new Schema("source");

            var targetSchema =
                new Schema("target")
                    .ConfigurePreviewUrls(previewUrls);

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            events.ShouldHaveSameEvents(
                new SchemaPreviewUrlsConfigured { PreviewUrls = previewUrls }
            );
        }

        [Fact]
        public void Should_create_events_if_schema_published()
        {
            var sourceSchema =
                new Schema("source");

            var targetSchema =
                new Schema("target")
                    .Publish();

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            events.ShouldHaveSameEvents(
                new SchemaPublished()
            );
        }

        [Fact]
        public void Should_create_events_if_schema_unpublished()
        {
            var sourceSchema =
                new Schema("source")
                    .Publish();

            var targetSchema =
                new Schema("target");

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            events.ShouldHaveSameEvents(
                new SchemaUnpublished()
            );
        }

        [Fact]
        public void Should_create_events_if_list_fields_changed()
        {
            var sourceSchema =
                new Schema("source")
                    .ConfigureFieldsInLists("1", "2");

            var targetSchema =
                new Schema("target")
                    .ConfigureFieldsInLists("2", "1");

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            events.ShouldHaveSameEvents(
                new SchemaUIFieldsConfigured { FieldsInLists = new FieldNames("2", "1") }
            );
        }

        [Fact]
        public void Should_create_events_if_reference_fields_changed()
        {
            var sourceSchema =
                new Schema("source")
                    .ConfigureFieldsInReferences("1", "2");

            var targetSchema =
                new Schema("target")
                    .ConfigureFieldsInReferences("2", "1");

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            events.ShouldHaveSameEvents(
                new SchemaUIFieldsConfigured { FieldsInReferences = new FieldNames("2", "1") }
            );
        }

        [Fact]
        public void Should_create_events_if_nested_field_deleted()
        {
            var sourceSchema =
                new Schema("source")
                    .AddArray(arrayId.Id, arrayId.Name, Partitioning.Invariant, f => f
                        .AddString(nestedId.Id, nestedId.Name));

            var targetSchema =
                new Schema("target")
                    .AddArray(arrayId.Id, arrayId.Name, Partitioning.Invariant);

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            events.ShouldHaveSameEvents(
                new FieldDeleted { FieldId = nestedId, ParentFieldId = arrayId }
            );
        }

        [Fact]
        public void Should_create_events_if_field_deleted()
        {
            var sourceSchema =
                new Schema("source")
                    .AddString(stringId.Id, stringId.Name, Partitioning.Invariant);

            var targetSchema =
                new Schema("target");

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            events.ShouldHaveSameEvents(
                new FieldDeleted { FieldId = stringId }
            );
        }

        [Fact]
        public void Should_create_events_if_nested_field_updated()
        {
            var properties = new StringFieldProperties { IsRequired = true };

            var sourceSchema =
                new Schema("source")
                    .AddArray(arrayId.Id, arrayId.Name, Partitioning.Invariant, f => f
                        .AddString(nestedId.Id, nestedId.Name));

            var targetSchema =
                new Schema("target")
                    .AddArray(arrayId.Id, arrayId.Name, Partitioning.Invariant, f => f
                        .AddString(nestedId.Id, nestedId.Name, properties));

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            events.ShouldHaveSameEvents(
                new FieldUpdated { Properties = properties, FieldId = nestedId, ParentFieldId = arrayId }
            );
        }

        [Fact]
        public void Should_create_events_if_field_updated()
        {
            var properties = new StringFieldProperties { IsRequired = true };

            var sourceSchema =
                new Schema("source")
                    .AddString(stringId.Id, stringId.Name, Partitioning.Invariant);

            var targetSchema =
                new Schema("target")
                    .AddString(stringId.Id, stringId.Name, Partitioning.Invariant, properties);

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            events.ShouldHaveSameEvents(
                new FieldUpdated { Properties = properties, FieldId = stringId }
            );
        }

        [Fact]
        public void Should_create_events_if_nested_field_locked()
        {
            var sourceSchema =
                new Schema("source")
                    .AddArray(arrayId.Id, arrayId.Name, Partitioning.Invariant, f => f
                        .AddString(nestedId.Id, nestedId.Name));

            var targetSchema =
                new Schema("target")
                    .AddArray(arrayId.Id, arrayId.Name, Partitioning.Invariant, f => f
                        .AddString(nestedId.Id, nestedId.Name))
                            .LockField(nestedId.Id, arrayId.Id);

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            events.ShouldHaveSameEvents(
                new FieldLocked { FieldId = nestedId, ParentFieldId = arrayId }
            );
        }

        [Fact]
        public void Should_create_events_if_field_locked()
        {
            var sourceSchema =
                new Schema("source")
                    .AddString(stringId.Id, stringId.Name, Partitioning.Invariant);

            var targetSchema =
                new Schema("target")
                    .AddString(stringId.Id, stringId.Name, Partitioning.Invariant)
                        .LockField(stringId.Id);

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            events.ShouldHaveSameEvents(
                new FieldLocked { FieldId = stringId }
            );
        }

        [Fact]
        public void Should_create_events_if_nested_field_hidden()
        {
            var sourceSchema =
                new Schema("source")
                    .AddArray(arrayId.Id, arrayId.Name, Partitioning.Invariant, f => f
                        .AddString(nestedId.Id, nestedId.Name));

            var targetSchema =
                new Schema("target")
                    .AddArray(arrayId.Id, arrayId.Name, Partitioning.Invariant, f => f
                        .AddString(nestedId.Id, nestedId.Name))
                            .HideField(nestedId.Id, arrayId.Id);

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            events.ShouldHaveSameEvents(
                new FieldHidden { FieldId = nestedId, ParentFieldId = arrayId }
            );
        }

        [Fact]
        public void Should_create_events_if_field_hidden()
        {
            var sourceSchema =
                new Schema("source")
                    .AddString(stringId.Id, stringId.Name, Partitioning.Invariant);

            var targetSchema =
                new Schema("target")
                    .AddString(stringId.Id, stringId.Name, Partitioning.Invariant)
                        .HideField(stringId.Id);

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            events.ShouldHaveSameEvents(
                new FieldHidden { FieldId = stringId }
            );
        }

        [Fact]
        public void Should_create_events_if_nested_field_shown()
        {
            var sourceSchema =
                new Schema("source")
                    .AddArray(arrayId.Id, arrayId.Name, Partitioning.Invariant, f => f
                        .AddString(nestedId.Id, nestedId.Name))
                            .HideField(nestedId.Id, arrayId.Id);

            var targetSchema =
                new Schema("target")
                    .AddArray(arrayId.Id, arrayId.Name, Partitioning.Invariant, f => f
                        .AddString(nestedId.Id, nestedId.Name));

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            events.ShouldHaveSameEvents(
                new FieldShown { FieldId = nestedId, ParentFieldId = arrayId }
            );
        }

        [Fact]
        public void Should_create_events_if_field_shown()
        {
            var sourceSchema =
                new Schema("source")
                    .AddString(stringId.Id, stringId.Name, Partitioning.Invariant)
                        .HideField(stringId.Id);

            var targetSchema =
                new Schema("target")
                    .AddString(stringId.Id, stringId.Name, Partitioning.Invariant);

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            events.ShouldHaveSameEvents(
                new FieldShown { FieldId = stringId }
            );
        }

        [Fact]
        public void Should_create_events_if_nested_field_disabled()
        {
            var sourceSchema =
                new Schema("source")
                    .AddArray(arrayId.Id, arrayId.Name, Partitioning.Invariant, f => f
                        .AddString(nestedId.Id, nestedId.Name));

            var targetSchema =
                new Schema("target")
                    .AddArray(arrayId.Id, arrayId.Name, Partitioning.Invariant, f => f
                        .AddString(nestedId.Id, nestedId.Name))
                            .DisableField(nestedId.Id, arrayId.Id);

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            events.ShouldHaveSameEvents(
                new FieldDisabled { FieldId = nestedId, ParentFieldId = arrayId }
            );
        }

        [Fact]
        public void Should_create_events_if_field_disabled()
        {
            var sourceSchema =
                new Schema("source")
                    .AddString(stringId.Id, stringId.Name, Partitioning.Invariant);

            var targetSchema =
                new Schema("target")
                    .AddString(stringId.Id, stringId.Name, Partitioning.Invariant)
                        .DisableField(stringId.Id);

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            events.ShouldHaveSameEvents(
                new FieldDisabled { FieldId = stringId }
            );
        }

        [Fact]
        public void Should_create_events_if_nested_field_enabled()
        {
            var sourceSchema =
                new Schema("source")
                    .AddArray(arrayId.Id, arrayId.Name, Partitioning.Invariant, f => f
                        .AddString(nestedId.Id, nestedId.Name))
                            .DisableField(nestedId.Id, arrayId.Id);

            var targetSchema =
                new Schema("target")
                    .AddArray(arrayId.Id, arrayId.Name, Partitioning.Invariant, f => f
                        .AddString(nestedId.Id, nestedId.Name));

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            events.ShouldHaveSameEvents(
                new FieldEnabled { FieldId = nestedId, ParentFieldId = arrayId }
            );
        }

        [Fact]
        public void Should_create_events_if_field_enabled()
        {
            var sourceSchema =
                new Schema("source")
                    .AddString(stringId.Id, stringId.Name, Partitioning.Invariant)
                        .DisableField(stringId.Id);

            var targetSchema =
                new Schema("target")
                    .AddString(stringId.Id, stringId.Name, Partitioning.Invariant);

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            events.ShouldHaveSameEvents(
                new FieldEnabled { FieldId = stringId }
            );
        }

        [Fact]
        public void Should_create_events_if_field_created()
        {
            var sourceSchema =
                new Schema("source");

            var targetSchema =
                new Schema("target")
                    .AddString(stringId.Id, stringId.Name, Partitioning.Invariant)
                        .HideField(stringId.Id);

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            var createdId = NamedId.Of(50L, stringId.Name);

            events.ShouldHaveSameEvents(
                new FieldAdded { FieldId = createdId, Name = stringId.Name, Partitioning = Partitioning.Invariant.Key, Properties = new StringFieldProperties() },
                new FieldHidden { FieldId = createdId }
            );
        }

        [Fact]
        public void Should_create_events_if_field_type_has_changed()
        {
            var sourceSchema =
                new Schema("source")
                    .AddString(stringId.Id, stringId.Name, Partitioning.Invariant);

            var targetSchema =
                new Schema("target")
                    .AddTags(stringId.Id, stringId.Name, Partitioning.Invariant);

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            var createdId = NamedId.Of(50L, stringId.Name);

            events.ShouldHaveSameEvents(
                new FieldDeleted { FieldId = stringId },
                new FieldAdded { FieldId = createdId, Name = stringId.Name, Partitioning = Partitioning.Invariant.Key, Properties = new TagsFieldProperties() }
            );
        }

        [Fact]
        public void Should_create_events_if_field_partitioning_has_changed()
        {
            var sourceSchema =
                new Schema("source")
                    .AddString(stringId.Id, stringId.Name, Partitioning.Invariant);

            var targetSchema =
                new Schema("target")
                    .AddString(stringId.Id, stringId.Name, Partitioning.Language);

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            var createdId = NamedId.Of(50L, stringId.Name);

            events.ShouldHaveSameEvents(
                new FieldDeleted { FieldId = stringId },
                new FieldAdded { FieldId = createdId, Name = stringId.Name, Partitioning = Partitioning.Language.Key, Properties = new StringFieldProperties() }
            );
        }

        [Fact]
        public void Should_create_events_if_nested_field_created()
        {
            var sourceSchema =
                new Schema("source");

            var targetSchema =
                new Schema("target")
                    .AddArray(arrayId.Id, arrayId.Name, Partitioning.Invariant, f => f
                        .AddString(nestedId.Id, nestedId.Name))
                            .HideField(nestedId.Id, arrayId.Id);

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            var id1 = NamedId.Of(50L, arrayId.Name);
            var id2 = NamedId.Of(51L, stringId.Name);

            events.ShouldHaveSameEvents(
                new FieldAdded { FieldId = id1, Name = arrayId.Name, Partitioning = Partitioning.Invariant.Key, Properties = new ArrayFieldProperties() },
                new FieldAdded { FieldId = id2, Name = stringId.Name, ParentFieldId = id1, Properties = new StringFieldProperties() },
                new FieldHidden { FieldId = id2, ParentFieldId = id1 }
            );
        }

        [Fact]
        public void Should_create_events_if_nested_fields_reordered()
        {
            var id1 = NamedId.Of(1, "f1");
            var id2 = NamedId.Of(2, "f1");

            var sourceSchema =
                new Schema("source")
                    .AddArray(arrayId.Id, arrayId.Name, Partitioning.Invariant, f => f
                        .AddString(10, "f1")
                        .AddString(11, "f2"));

            var targetSchema =
                new Schema("target")
                    .AddArray(arrayId.Id, arrayId.Name, Partitioning.Invariant, f => f
                        .AddString(1, "f2")
                        .AddString(2, "f1"));

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            events.ShouldHaveSameEvents(
                new SchemaFieldsReordered { FieldIds = new List<long> { 11, 10 }, ParentFieldId = arrayId }
            );
        }

        [Fact]
        public void Should_create_events_if_fields_reordered()
        {
            var id1 = NamedId.Of(1, "f1");
            var id2 = NamedId.Of(2, "f1");

            var sourceSchema =
                new Schema("source")
                    .AddString(10, "f1", Partitioning.Invariant)
                    .AddString(11, "f2", Partitioning.Invariant);

            var targetSchema =
                new Schema("target")
                    .AddString(1, "f2", Partitioning.Invariant)
                    .AddString(2, "f1", Partitioning.Invariant);

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            events.ShouldHaveSameEvents(
                new SchemaFieldsReordered { FieldIds = new List<long> { 11, 10 } }
            );
        }

        [Fact]
        public void Should_create_events_if_fields_reordered_after_sync()
        {
            var id1 = NamedId.Of(1, "f1");
            var id2 = NamedId.Of(2, "f1");

            var sourceSchema =
                new Schema("source")
                    .AddString(10, "f1", Partitioning.Invariant)
                    .AddString(11, "f2", Partitioning.Invariant);

            var targetSchema =
                new Schema("target")
                    .AddString(1, "f3", Partitioning.Invariant)
                    .AddString(2, "f1", Partitioning.Invariant);

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            events.ShouldHaveSameEvents(
                new FieldDeleted { FieldId = NamedId.Of(11L, "f2") },
                new FieldAdded { FieldId = NamedId.Of(50L, "f3"), Name = "f3", Partitioning = Partitioning.Invariant.Key, Properties = new StringFieldProperties() },
                new SchemaFieldsReordered { FieldIds = new List<long> { 50, 10 } }
            );
        }

        [Fact]
        public void Should_create_events_if_fields_reordered_after_sync2()
        {
            var id1 = NamedId.Of(1, "f1");
            var id2 = NamedId.Of(2, "f1");

            var sourceSchema =
                new Schema("source")
                    .AddString(10, "f1", Partitioning.Invariant)
                    .AddString(11, "f2", Partitioning.Invariant);

            var targetSchema =
                new Schema("target")
                    .AddString(1, "f1", Partitioning.Invariant)
                    .AddString(2, "f3", Partitioning.Invariant)
                    .AddString(3, "f2", Partitioning.Invariant);

            var events = sourceSchema.Synchronize(targetSchema, jsonSerializer, idGenerator);

            events.ShouldHaveSameEvents(
                new FieldAdded { FieldId = NamedId.Of(50L, "f3"), Name = "f3", Partitioning = Partitioning.Invariant.Key, Properties = new StringFieldProperties() },
                new SchemaFieldsReordered { FieldIds = new List<long> { 10, 50, 11 } }
            );
        }
    }
}
