// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Schemas.DomainObject;

public sealed partial class SchemaDomainObject
{
    protected override Schema Apply(Schema snapshot, Envelope<IEvent> @event)
    {
        var newSnapshot = snapshot;

        switch (@event.Payload)
        {
            case SchemaCreated e:
                newSnapshot = e.Schema with
                {
                    Id = e.SchemaId.Id,
                    // The schema usually does not contain any metadata.
                    AppId = e.AppId,
                    // Just update the total count to be more reliabble.
                    SchemaFieldsTotal = e.Schema.MaxId()
                };
                break;

            case FieldAdded e:
                if (e.ParentFieldId != null)
                {
                    var field = e.Properties.CreateNestedField(e.FieldId.Id, e.Name);

                    newSnapshot = newSnapshot.UpdateField(e.ParentFieldId.Id, x => ((ArrayField)x).AddField(field));
                }
                else
                {
                    var partitioning = Partitioning.FromString(e.Partitioning);

                    var field = e.Properties.CreateRootField(e.FieldId.Id, e.Name, partitioning);

                    newSnapshot = newSnapshot.DeleteField(e.FieldId.Id);
                    newSnapshot = newSnapshot.AddField(field);
                }

                newSnapshot = newSnapshot with { SchemaFieldsTotal = newSnapshot.MaxId() };
                break;

            case SchemaUIFieldsConfigured e:
                if (e.FieldsInLists != null)
                {
                    newSnapshot = newSnapshot.SetFieldsInLists(e.FieldsInLists);
                }

                if (e.FieldsInReferences != null)
                {
                    newSnapshot = newSnapshot.SetFieldsInReferences(e.FieldsInReferences);
                }

                break;

            case SchemaCategoryChanged e:
                newSnapshot = newSnapshot.ChangeCategory(e.Name);
                break;

            case SchemaScriptsConfigured e:
                newSnapshot = newSnapshot.SetScripts(e.Scripts ?? new ());
                break;

            case SchemaPreviewUrlsConfigured e:
                newSnapshot = newSnapshot.SetPreviewUrls(e.PreviewUrls ?? new ());
                break;

            case SchemaFieldRulesConfigured e:
                newSnapshot = newSnapshot.SetFieldRules(e.FieldRules ?? FieldRules.Empty);
                break;

            case SchemaPublished:
                newSnapshot = newSnapshot.Publish();
                break;

            case SchemaUnpublished:
                newSnapshot = newSnapshot.Unpublish();
                break;

            case SchemaUpdated e:
                newSnapshot = newSnapshot.Update(e.Properties);
                break;

            case SchemaFieldsReordered e:
                newSnapshot = newSnapshot.ReorderFields(e.FieldIds.ToList(), e.ParentFieldId?.Id);
                break;

            case FieldUpdated e:
                newSnapshot = newSnapshot.UpdateField(e.FieldId.Id, e.Properties, e.ParentFieldId?.Id);
                break;

            case FieldLocked e:
                newSnapshot = newSnapshot.LockField(e.FieldId.Id, e.ParentFieldId?.Id);
                break;

            case FieldDisabled e:
                newSnapshot = newSnapshot.DisableField(e.FieldId.Id, e.ParentFieldId?.Id);
                break;

            case FieldEnabled e:
                newSnapshot = newSnapshot.EnableField(e.FieldId.Id, e.ParentFieldId?.Id);
                break;

            case FieldHidden e:
                newSnapshot = newSnapshot.HideField(e.FieldId.Id, e.ParentFieldId?.Id);
                break;

            case FieldShown e:
                newSnapshot = newSnapshot.ShowField(e.FieldId.Id, e.ParentFieldId?.Id);
                break;

            case FieldDeleted e:
                newSnapshot = newSnapshot.DeleteField(e.FieldId.Id, e.ParentFieldId?.Id);
                break;

            case SchemaDeleted:
                newSnapshot = newSnapshot with { IsDeleted = true };
                break;
        }

        if (ReferenceEquals(newSnapshot, snapshot))
        {
            return snapshot;
        }

        return newSnapshot.Apply(@event.To<SquidexEvent>());
    }
}
