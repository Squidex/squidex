﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;
using SchemaCreatedV2 = Squidex.Domain.Apps.Events.Schemas.SchemaCreated;
using SchemaFields = System.Collections.Generic.List<Squidex.Domain.Apps.Events.Schemas.SchemaCreatedField>;

namespace Migrate_01.OldEvents
{
    [EventType(nameof(SchemaCreated))]
    [Obsolete]
    public sealed class SchemaCreated : SchemaEvent, IMigrated<IEvent>
    {
        public string Name { get; set; }

        public bool Singleton { get; set; }

        public bool Publish { get; set; }

        public SchemaFields Fields { get; set; }

        public SchemaProperties Properties { get; set; }

        public IEvent Migrate()
        {
            var schema = new Schema(Name, Properties, Singleton);

            if (Publish)
            {
                schema = schema.Publish();
            }

            var totalFields = 0;

            if (Fields != null)
            {
                foreach (var eventField in Fields)
                {
                    totalFields++;

                    var partitioning = Partitioning.FromString(eventField.Partitioning);

                    var field = eventField.Properties.CreateRootField(totalFields, eventField.Name, partitioning);

                    if (field is ArrayField arrayField && eventField.Nested?.Count > 0)
                    {
                        foreach (var nestedEventField in eventField.Nested)
                        {
                            totalFields++;

                            var nestedField = nestedEventField.Properties.CreateNestedField(totalFields, nestedEventField.Name);

                            if (nestedEventField.IsHidden)
                            {
                                nestedField = nestedField.Hide();
                            }

                            if (nestedEventField.IsDisabled)
                            {
                                nestedField = nestedField.Disable();
                            }

                            if (nestedEventField.IsLocked)
                            {
                                nestedField = nestedField.Lock();
                            }

                            arrayField = arrayField.AddField(nestedField);
                        }

                        field = arrayField;
                    }

                    if (eventField.IsHidden)
                    {
                        field = field.Hide();
                    }

                    if (eventField.IsDisabled)
                    {
                        field = field.Disable();
                    }

                    if (eventField.IsLocked)
                    {
                        field = field.Lock();
                    }

                    schema = schema.AddField(field);
                }
            }

            return SimpleMapper.Map(this, new SchemaCreatedV2 { Schema = schema });
        }
    }
}
