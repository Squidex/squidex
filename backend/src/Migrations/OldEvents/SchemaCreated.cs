﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;
using SchemaCreatedV2 = Squidex.Domain.Apps.Events.Schemas.SchemaCreated;
using SchemaFields = System.Collections.Generic.List<Squidex.Domain.Apps.Events.Schemas.SchemaCreatedField>;

namespace Migrations.OldEvents;

[EventType(nameof(SchemaCreated))]
[Obsolete("New Event introduced")]
public sealed class SchemaCreated : SchemaEvent, IMigrated<IEvent>
{
    public string Name { get; set; }

    public bool Singleton { get; set; }

    public bool Publish { get; set; }

    public SchemaFields Fields { get; set; }

    public SchemaProperties Properties { get; set; }

    public IEvent Migrate()
    {
        var fields = new List<RootField>();

        if (Fields != null)
        {
            var totalFields = 0;

            foreach (var eventField in Fields)
            {
                totalFields++;

                var field = eventField.Properties.CreateRootField(totalFields, eventField.Name,
                    Partitioning.FromString(eventField.Partitioning)) with
                {
                    IsLocked = eventField.IsLocked,
                    IsHidden = eventField.IsHidden,
                    IsDisabled = eventField.IsDisabled,
                };

                if (field is ArrayField arrayField && eventField.Nested?.Length > 0)
                {
                    var arrayFields = new List<NestedField>();

                    foreach (var nestedEventField in eventField.Nested)
                    {
                        totalFields++;

                        var nestedField = nestedEventField.Properties.CreateNestedField(totalFields, nestedEventField.Name) with
                        {
                            IsLocked = nestedEventField.IsLocked,
                            IsHidden = nestedEventField.IsHidden,
                            IsDisabled = nestedEventField.IsDisabled,
                        };

                        arrayFields.Add(nestedField);
                    }

                    field = arrayField with { FieldCollection = FieldCollection<NestedField>.Create(arrayFields.ToArray()) };
                }

                fields.Add(field);
            }
        }

        var schema = new Schema
        {
            Name = Name,
            Type = Singleton ?
                SchemaType.Singleton :
                SchemaType.Default,
            IsPublished = Publish,
            FieldCollection = FieldCollection<RootField>.Create(fields.ToArray()),
        };

        return SimpleMapper.Map(this, new SchemaCreatedV2 { Schema = schema });
    }
}
