// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.States;

#pragma warning disable IDE0060 // Remove unused parameter

namespace Squidex.Domain.Apps.Entities.Schemas.State
{
    [CollectionName("Schemas")]
    public sealed class SchemaState : DomainObjectState<SchemaState>, ISchemaEntity
    {
        public NamedId<Guid> AppId { get; set; }

        public Schema SchemaDef { get; set; }

        public long SchemaFieldsTotal { get; set; }

        public override bool ApplyEvent(IEvent @event)
        {
            var previousSchema = SchemaDef;

            switch (@event)
            {
                case SchemaCreated e:
                    {
                        SchemaDef = e.Schema;
                        SchemaFieldsTotal = e.Schema.MaxId();

                        AppId = e.AppId;

                        return true;
                    }

                case FieldAdded e:
                    {
                        if (e.ParentFieldId != null)
                        {
                            var field = e.Properties.CreateNestedField(e.FieldId.Id, e.Name);

                            SchemaDef = SchemaDef.UpdateField(e.ParentFieldId.Id, x => ((ArrayField)x).AddField(field));
                        }
                        else
                        {
                            var partitioning = Partitioning.FromString(e.Partitioning);

                            var field = e.Properties.CreateRootField(e.FieldId.Id, e.Name, partitioning);

                            SchemaDef = SchemaDef.DeleteField(e.FieldId.Id);
                            SchemaDef = SchemaDef.AddField(field);
                        }

                        SchemaFieldsTotal = Math.Max(SchemaFieldsTotal, e.FieldId.Id);

                        break;
                    }

                case SchemaUIFieldsConfigured e:
                    {
                        if (e.FieldsInLists != null)
                        {
                            SchemaDef = SchemaDef.SetFieldsInLists(e.FieldsInLists);
                        }

                        if (e.FieldsInReferences != null)
                        {
                            SchemaDef = SchemaDef.SetFieldsInReferences(e.FieldsInReferences);
                        }

                        break;
                    }

                case SchemaCategoryChanged e:
                    {
                        SchemaDef = SchemaDef.ChangeCategory(e.Name);

                        break;
                    }

                case SchemaPreviewUrlsConfigured e:
                    {
                        SchemaDef = SchemaDef.SetPreviewUrls(e.PreviewUrls);

                        break;
                    }

                case SchemaScriptsConfigured e:
                    {
                        SchemaDef = SchemaDef.SetScripts(e.Scripts);

                        break;
                    }

                case SchemaPublished _:
                    {
                        SchemaDef = SchemaDef.Publish();

                        break;
                    }

                case SchemaUnpublished _:
                    {
                        SchemaDef = SchemaDef.Unpublish();

                        break;
                    }

                case SchemaUpdated e:
                    {
                        SchemaDef = SchemaDef.Update(e.Properties);

                        break;
                    }

                case SchemaFieldsReordered e:
                    {
                        SchemaDef = SchemaDef.ReorderFields(e.FieldIds, e.ParentFieldId?.Id);

                        break;
                    }

                case FieldUpdated e:
                    {
                        SchemaDef = SchemaDef.UpdateField(e.FieldId.Id, e.Properties, e.ParentFieldId?.Id);

                        break;
                    }

                case FieldLocked e:
                    {
                        SchemaDef = SchemaDef.LockField(e.FieldId.Id, e.ParentFieldId?.Id);

                        break;
                    }

                case FieldDisabled e:
                    {
                        SchemaDef = SchemaDef.DisableField(e.FieldId.Id, e.ParentFieldId?.Id);

                        break;
                    }

                case FieldEnabled e:
                    {
                        SchemaDef = SchemaDef.EnableField(e.FieldId.Id, e.ParentFieldId?.Id);

                        break;
                    }

                case FieldHidden e:
                    {
                        SchemaDef = SchemaDef.HideField(e.FieldId.Id, e.ParentFieldId?.Id);

                        break;
                    }

                case FieldShown e:
                    {
                        SchemaDef = SchemaDef.ShowField(e.FieldId.Id, e.ParentFieldId?.Id);

                        break;
                    }

                case FieldDeleted e:
                    {
                        SchemaDef = SchemaDef.DeleteField(e.FieldId.Id, e.ParentFieldId?.Id);

                        break;
                    }

                case SchemaDeleted _:
                    {
                        IsDeleted = true;

                        return true;
                    }
            }

            return !ReferenceEquals(previousSchema, SchemaDef);
        }
    }
}
